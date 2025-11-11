using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

public static class SessionCalculator
{
    public static double? CalculatePrice(ParkingLotModel parkingLot, ReservationModel reservation)
    {
        double? totalPrice = 0.0;
        DateTime endTime = reservation.EndTime ?? DateTime.Now;
        TimeSpan duration = endTime - reservation.StartTime;

        int hours = (int)Math.Ceiling(duration.TotalHours);

        if (endTime.Date > reservation.StartTime.Date)
        {
            totalPrice = (parkingLot.DayTariff * (endTime.Date - reservation.StartTime.Date).Days);
        }
        else
        {
            totalPrice = parkingLot.DayTariff * duration.Hours;

            if (totalPrice > parkingLot.DayTariff)
            {
                totalPrice = (double)parkingLot.DayTariff;
            }
        }

        return totalPrice;
    }

    public static string GeneratePaymentHash(string userName, DateTime dateTime)
    {
        byte[] combination = System.Text.Encoding.UTF8.GetBytes(userName + dateTime.ToString("o"));

        MD5 md5;

        using (md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(combination);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    public static string GeneratePaymentHash(ParkingSessionModel session)
    {
        return GeneratePaymentHash(session.User, DateTime.Now);
    }

    public static string GenerateTransactionValidationHash()
    {
        return Guid.NewGuid().ToString("N");
    }

    public static double CheckPaymentAmount(string paymentHash)
    {
        throw new NotImplementedException();
    }
}