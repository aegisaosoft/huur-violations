# Stripe Integration Guide for Car Rental System

## Overview
This guide covers implementing Stripe payment processing for a multi-company car rental platform. We'll use Stripe Connect for marketplace functionality where multiple rental companies can receive payments.

## Architecture Choice

### Option 1: Stripe Connect (Recommended for Multi-Company)
- Each rental company has their own Stripe Connect account
- Platform takes a fee from each transaction
- Companies receive payouts directly to their bank accounts
- Best for true marketplace model

### Option 2: Direct Charges (Simpler)
- All payments go to single platform Stripe account
- Platform manually pays out to rental companies
- Simpler to implement but more manual work

**This guide covers Option 1 (Stripe Connect)**

## Setup Steps

### 1. Create Stripe Account
```bash
# Install Stripe CLI for testing
brew install stripe/stripe-cli/stripe

# Login to Stripe
stripe login
```

### 2. Environment Variables
```env
# .env file
STRIPE_SECRET_KEY=sk_test_...
STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...
STRIPE_CONNECT_CLIENT_ID=ca_...
```

### 3. Database Setup
The schema includes:
- `rental_companies.stripe_account_id` - Stores connected account ID
- `customers.stripe_customer_id` - Stores customer ID
- `payments` table - Tracks all payment transactions
- `customer_payment_methods` table - Stores saved payment methods

## Implementation Flow

### Phase 1: Company Onboarding (Stripe Connect)

#### Step 1: Generate Connect Link
```javascript
// Node.js/Express example
const stripe = require('stripe')(process.env.STRIPE_SECRET_KEY);

// Generate OAuth link for rental company to connect
app.get('/api/rental-company/connect-stripe', async (req, res) => {
  const { companyId } = req.user; // From auth middleware
  
  const authUrl = `https://connect.stripe.com/oauth/authorize?` +
    `response_type=code&` +
    `client_id=${process.env.STRIPE_CONNECT_CLIENT_ID}&` +
    `scope=read_write&` +
    `state=${companyId}`;
  
  res.json({ url: authUrl });
});

// Handle OAuth callback
app.get('/api/stripe/callback', async (req, res) => {
  const { code, state: companyId } = req.query;
  
  try {
    const response = await stripe.oauth.token({
      grant_type: 'authorization_code',
      code,
    });
    
    // Save the connected account ID
    await db.query(
      'UPDATE rental_companies SET stripe_account_id = $1 WHERE company_id = $2',
      [response.stripe_user_id, companyId]
    );
    
    res.redirect('/dashboard?stripe_connected=true');
  } catch (error) {
    console.error('Stripe connection error:', error);
    res.redirect('/dashboard?stripe_error=true');
  }
});
```

### Phase 2: Customer Setup

#### Step 2: Create Stripe Customer
```javascript
// Create customer when they first register
app.post('/api/customers/register', async (req, res) => {
  const { email, firstName, lastName, phone } = req.body;
  
  try {
    // Create Stripe customer
    const stripeCustomer = await stripe.customers.create({
      email,
      name: `${firstName} ${lastName}`,
      phone,
      metadata: {
        source: 'car_rental_platform'
      }
    });
    
    // Save to database
    const result = await db.query(
      `INSERT INTO customers 
       (email, first_name, last_name, phone, stripe_customer_id) 
       VALUES ($1, $2, $3, $4, $5) 
       RETURNING customer_id`,
      [email, firstName, lastName, phone, stripeCustomer.id]
    );
    
    res.json({ 
      customerId: result.rows[0].customer_id,
      message: 'Customer created successfully'
    });
  } catch (error) {
    console.error('Customer creation error:', error);
    res.status(500).json({ error: 'Failed to create customer' });
  }
});
```

### Phase 3: Reservation & Payment

#### Step 3: Create Reservation with Payment Intent
```javascript
app.post('/api/reservations/create', async (req, res) => {
  const {
    customerId,
    vehicleId,
    pickupDate,
    returnDate,
    pickupLocation,
    returnLocation
  } = req.body;
  
  try {
    // 1. Get vehicle and company details
    const vehicleQuery = await db.query(
      `SELECT v.*, rc.stripe_account_id, v.daily_rate
       FROM vehicles v
       JOIN rental_companies rc ON v.company_id = rc.company_id
       WHERE v.vehicle_id = $1 AND v.status = 'available'`,
      [vehicleId]
    );
    
    if (vehicleQuery.rows.length === 0) {
      return res.status(400).json({ error: 'Vehicle not available' });
    }
    
    const vehicle = vehicleQuery.rows[0];
    
    // 2. Get customer Stripe ID
    const customerQuery = await db.query(
      'SELECT stripe_customer_id FROM customers WHERE customer_id = $1',
      [customerId]
    );
    
    const stripeCustomerId = customerQuery.rows[0].stripe_customer_id;
    
    // 3. Calculate pricing
    const pickup = new Date(pickupDate);
    const returnDt = new Date(returnDate);
    const totalDays = Math.ceil((returnDt - pickup) / (1000 * 60 * 60 * 24));
    const subtotal = vehicle.daily_rate * totalDays;
    const taxRate = 0.10; // 10% tax
    const taxAmount = subtotal * taxRate;
    const totalAmount = subtotal + taxAmount;
    
    // 4. Create reservation in database
    const reservationNumber = `RES${Date.now()}`;
    const reservation = await db.query(
      `INSERT INTO reservations 
       (customer_id, vehicle_id, company_id, reservation_number,
        pickup_date, return_date, pickup_location, return_location,
        daily_rate, total_days, subtotal, tax_amount, total_amount, status)
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, 'pending')
       RETURNING *`,
      [
        customerId, vehicleId, vehicle.company_id, reservationNumber,
        pickupDate, returnDate, pickupLocation, returnLocation,
        vehicle.daily_rate, totalDays, subtotal, taxAmount, totalAmount
      ]
    );
    
    // 5. Create Stripe Payment Intent with Connect
    const platformFeePercent = 0.15; // 15% platform fee
    const platformFeeAmount = Math.round(totalAmount * 100 * platformFeePercent);
    
    const paymentIntent = await stripe.paymentIntents.create({
      amount: Math.round(totalAmount * 100), // Amount in cents
      currency: 'usd',
      customer: stripeCustomerId,
      application_fee_amount: platformFeeAmount,
      transfer_data: {
        destination: vehicle.stripe_account_id, // Connected account
      },
      metadata: {
        reservation_id: reservation.rows[0].reservation_id,
        reservation_number: reservationNumber,
        customer_id: customerId,
        vehicle_id: vehicleId
      },
      automatic_payment_methods: {
        enabled: true,
      },
    });
    
    // 6. Record payment in database
    await db.query(
      `INSERT INTO payments 
       (reservation_id, customer_id, company_id, amount, currency,
        payment_type, stripe_payment_intent_id, status)
       VALUES ($1, $2, $3, $4, $5, $6, $7, 'pending')`,
      [
        reservation.rows[0].reservation_id,
        customerId,
        vehicle.company_id,
        totalAmount,
        'USD',
        'full_payment',
        paymentIntent.id
      ]
    );
    
    res.json({
      reservationId: reservation.rows[0].reservation_id,
      reservationNumber,
      clientSecret: paymentIntent.client_secret,
      totalAmount,
      totalDays
    });
    
  } catch (error) {
    console.error('Reservation creation error:', error);
    res.status(500).json({ error: 'Failed to create reservation' });
  }
});
```

#### Step 4: Frontend Payment Form (React)
```javascript
// PaymentForm.jsx
import { useStripe, useElements, PaymentElement } from '@stripe/react-stripe-js';
import { useState } from 'react';

export default function PaymentForm({ clientSecret, reservationId }) {
  const stripe = useStripe();
  const elements = useElements();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!stripe || !elements) {
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const { error: submitError } = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: `${window.location.origin}/reservation-confirmation/${reservationId}`,
        },
      });
      
      if (submitError) {
        setError(submitError.message);
      }
    } catch (err) {
      setError('Payment failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <PaymentElement />
      <button type="submit" disabled={!stripe || loading}>
        {loading ? 'Processing...' : 'Pay Now'}
      </button>
      {error && <div className="error">{error}</div>}
    </form>
  );
}

// App.jsx wrapper
import { Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';

const stripePromise = loadStripe(process.env.REACT_APP_STRIPE_PUBLISHABLE_KEY);

function CheckoutPage({ clientSecret, reservationId }) {
  return (
    <Elements stripe={stripePromise} options={{ clientSecret }}>
      <PaymentForm clientSecret={clientSecret} reservationId={reservationId} />
    </Elements>
  );
}
```

### Phase 4: Webhooks (Critical!)

#### Step 5: Handle Stripe Webhooks
```javascript
// Webhook endpoint - MUST be separate from other middleware
app.post('/api/webhooks/stripe', 
  express.raw({ type: 'application/json' }),
  async (req, res) => {
    const sig = req.headers['stripe-signature'];
    let event;
    
    try {
      event = stripe.webhooks.constructEvent(
        req.body,
        sig,
        process.env.STRIPE_WEBHOOK_SECRET
      );
    } catch (err) {
      console.error('Webhook signature verification failed:', err.message);
      return res.status(400).send(`Webhook Error: ${err.message}`);
    }
    
    // Handle the event
    switch (event.type) {
      case 'payment_intent.succeeded':
        await handlePaymentSuccess(event.data.object);
        break;
        
      case 'payment_intent.payment_failed':
        await handlePaymentFailure(event.data.object);
        break;
        
      case 'charge.refunded':
        await handleRefund(event.data.object);
        break;
        
      default:
        console.log(`Unhandled event type ${event.type}`);
    }
    
    res.json({ received: true });
});

async function handlePaymentSuccess(paymentIntent) {
  const { reservation_id } = paymentIntent.metadata;
  
  try {
    // Start transaction
    await db.query('BEGIN');
    
    // Update payment status
    await db.query(
      `UPDATE payments 
       SET status = 'succeeded', 
           stripe_charge_id = $1,
           processed_at = CURRENT_TIMESTAMP
       WHERE stripe_payment_intent_id = $2`,
      [paymentIntent.latest_charge, paymentIntent.id]
    );
    
    // Update reservation status
    await db.query(
      `UPDATE reservations 
       SET status = 'confirmed'
       WHERE reservation_id = $1`,
      [reservation_id]
    );
    
    // Update vehicle status to reserved
    await db.query(
      `UPDATE vehicles 
       SET status = 'rented'
       WHERE vehicle_id = (
         SELECT vehicle_id FROM reservations WHERE reservation_id = $1
       )`,
      [reservation_id]
    );
    
    await db.query('COMMIT');
    
    // Send confirmation email (implement separately)
    // await sendReservationConfirmation(reservation_id);
    
  } catch (error) {
    await db.query('ROLLBACK');
    console.error('Payment success handler error:', error);
  }
}

async function handlePaymentFailure(paymentIntent) {
  const { reservation_id } = paymentIntent.metadata;
  
  try {
    await db.query(
      `UPDATE payments 
       SET status = 'failed', 
           failure_reason = $1
       WHERE stripe_payment_intent_id = $2`,
      [paymentIntent.last_payment_error?.message, paymentIntent.id]
    );
    
    await db.query(
      `UPDATE reservations 
       SET status = 'cancelled'
       WHERE reservation_id = $1`,
      [reservation_id]
    );
    
    // Send failure notification
    // await sendPaymentFailureEmail(reservation_id);
    
  } catch (error) {
    console.error('Payment failure handler error:', error);
  }
}

async function handleRefund(charge) {
  try {
    await db.query(
      `INSERT INTO payments 
       (customer_id, company_id, reservation_id, amount, currency,
        payment_type, stripe_charge_id, status, processed_at)
       SELECT customer_id, company_id, reservation_id, 
              -amount, currency, 'refund', $1, 'succeeded', CURRENT_TIMESTAMP
       FROM payments 
       WHERE stripe_charge_id = $1`,
      [charge.id]
    );
  } catch (error) {
    console.error('Refund handler error:', error);
  }
}
```

### Phase 5: Additional Features

#### Save Payment Methods
```javascript
app.post('/api/customers/payment-methods', async (req, res) => {
  const { customerId, paymentMethodId } = req.body;
  
  try {
    const customer = await db.query(
      'SELECT stripe_customer_id FROM customers WHERE customer_id = $1',
      [customerId]
    );
    
    // Attach payment method to customer
    await stripe.paymentMethods.attach(paymentMethodId, {
      customer: customer.rows[0].stripe_customer_id,
    });
    
    // Get payment method details
    const paymentMethod = await stripe.paymentMethods.retrieve(paymentMethodId);
    
    // Save to database
    await db.query(
      `INSERT INTO customer_payment_methods 
       (customer_id, stripe_payment_method_id, card_brand, 
        card_last4, card_exp_month, card_exp_year)
       VALUES ($1, $2, $3, $4, $5, $6)`,
      [
        customerId,
        paymentMethodId,
        paymentMethod.card.brand,
        paymentMethod.card.last4,
        paymentMethod.card.exp_month,
        paymentMethod.card.exp_year
      ]
    );
    
    res.json({ success: true });
  } catch (error) {
    console.error('Save payment method error:', error);
    res.status(500).json({ error: 'Failed to save payment method' });
  }
});
```

#### Process Refund
```javascript
app.post('/api/payments/refund', async (req, res) => {
  const { reservationId, amount, reason } = req.body;
  
  try {
    // Get payment details
    const payment = await db.query(
      `SELECT stripe_payment_intent_id 
       FROM payments 
       WHERE reservation_id = $1 AND status = 'succeeded'`,
      [reservationId]
    );
    
    // Create refund
    const refund = await stripe.refunds.create({
      payment_intent: payment.rows[0].stripe_payment_intent_id,
      amount: amount ? Math.round(amount * 100) : undefined, // Partial or full
      reason: reason || 'requested_by_customer',
    });
    
    res.json({ 
      success: true, 
      refundId: refund.id 
    });
  } catch (error) {
    console.error('Refund error:', error);
    res.status(500).json({ error: 'Refund failed' });
  }
});
```

## Testing

### 1. Test Cards
```
Success: 4242 4242 4242 4242
Decline: 4000 0000 0000 0002
Requires Authentication: 4000 0025 0000 3155
```

### 2. Test Webhook Locally
```bash
# Forward webhooks to local server
stripe listen --forward-to localhost:3000/api/webhooks/stripe

# Trigger test events
stripe trigger payment_intent.succeeded
```

## Security Considerations

1. **Never expose secret keys** - Keep them server-side only
2. **Validate webhook signatures** - Always verify Stripe webhooks
3. **Use HTTPS** - Required for production
4. **PCI Compliance** - Never handle raw card data (Stripe handles this)
5. **Idempotency** - Use idempotency keys for create operations
6. **Database transactions** - Use transactions for payment + reservation updates

## Production Checklist

- [ ] Switch to live Stripe keys
- [ ] Configure production webhook endpoint
- [ ] Set up proper error logging (Sentry, etc.)
- [ ] Implement email notifications
- [ ] Add fraud prevention rules in Stripe Dashboard
- [ ] Test refund flow thoroughly
- [ ] Set up monitoring for failed payments
- [ ] Configure Stripe radar for fraud detection
- [ ] Review and adjust platform fee percentage
- [ ] Implement proper rate limiting

## Common Issues & Solutions

### Issue: Payment succeeds but reservation not confirmed
**Solution**: Webhook not configured. Always rely on webhooks, not client-side success.

### Issue: Connected account not receiving funds
**Solution**: Check `transfer_data.destination` is set correctly in PaymentIntent.

### Issue: "No such customer" error
**Solution**: Ensure Stripe customer ID is saved correctly during registration.

### Issue: Webhook signature verification fails
**Solution**: Use raw body for webhook endpoint, not JSON parsed body.

## Resources

- [Stripe Connect Documentation](https://stripe.com/docs/connect)
- [Stripe Payment Intents API](https://stripe.com/docs/payments/payment-intents)
- [Stripe Webhooks Guide](https://stripe.com/docs/webhooks)
- [Stripe Testing](https://stripe.com/docs/testing)
