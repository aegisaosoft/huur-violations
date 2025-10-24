/*
 *
 * Copyright (c) 2025 Alexander Orlov.
 * 34 Middletown Ave Atlantic Highlands NJ 07716
 *
 * THIS SOFTWARE IS THE CONFIDENTIAL AND PROPRIETARY INFORMATION OF
 * Alexander Orlov. ("CONFIDENTIAL INFORMATION"). YOU SHALL NOT DISCLOSE
 * SUCH CONFIDENTIAL INFORMATION AND SHALL USE IT ONLY IN ACCORDANCE
 * WITH THE TERMS OF THE LICENSE AGREEMENT YOU ENTERED INTO WITH
 * Alexander Orlov.
 *
 * Author: Alexander Orlov
 *
 */

namespace HuurVioaltionsAPI.Helpers
{
    public class Helper
    {
        public static int GetStaus(string? status)
        {
            if (string.IsNullOrEmpty(status)) return 1;

            return status.ToUpper() switch
            {
                "OPEN" or "UNPAID" => Const.P_NEW,
                "PAID" => Const.P_PAID,
                "VOID" => Const.P_PAID,
                "PENDING" => Const.P_PAID,
                "OVERDUE" => Const.P_NEW,
                "CLOSED VOID" => Const.P_PAID,
                "CLOSED WARNING" => Const.P_PAID,
                "CLOSED PAID" => Const.P_PAID,
                _ => 1
            };
        }
    }
}
