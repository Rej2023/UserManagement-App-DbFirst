using System;
using System.Data.SqlClient;

namespace UserManagement_App_DbFirst
{
    public static class SqlExtensions
    {
        public static object OptionalParam<T>(this T value, T optionalValue = default(T))
        {
            return Equals(value, optionalValue) ? (object)DBNull.Value : value;
        }

        public static long DefaultLong(this long? value)
        {
            if (!value.HasValue)
            {
                return 0;
            }

            return value.Value;
        }

        public static String EmptyStringIfNull(this String value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            return value.Trim();
        }

        public static String NullIfEmptyString(this String value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value.Trim();
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader dr, int col)
        {
            if (dr.IsDBNull(col))
            {
                return null;
            }

            return dr.GetDateTime(col);
        }

        public static DateTime GetNullableDateTimeWithDefault(this SqlDataReader dr, int col, DateTime? defaultValue = null)
        {
            if (dr.IsDBNull(col))
            {
                return defaultValue.Value;
            }

            return dr.GetDateTime(col);
        }


        public static Decimal? GetNullableDecimal(this SqlDataReader dr, int col)
        {
            if (dr.IsDBNull(col))
            {
                return null;
            }

            return dr.GetDecimal(col);
        }

        public static int? GetNullableInt32(this SqlDataReader dr, int col)
        {
            if (dr.IsDBNull(col))
            {
                return null;
            }

            return dr.GetInt32(col);
        }

        public static long? GetNullableLong(this SqlDataReader dr, int col)
        {
            if (dr.IsDBNull(col))
            {
                return null;
            }

            return (long)dr.GetDecimal(col);
        }

        public static long GetNullableLong(this SqlDataReader dr, int col, long defaultValue)
        {
            if (dr.IsDBNull(col))
            {
                return defaultValue;
            }

            return (long)dr.GetDecimal(col);
        }


        public static long GetLong(this SqlDataReader dr, int col)
        {
            return (long)dr.GetDecimal(col);
        }

        public static int GetInt(this SqlDataReader dr, int col)
        {
            return (int)dr.GetDecimal(col);
        }

        public static String GetNullableString(this SqlDataReader dr, int col, String defaultValue = null)
        {
            if (dr.IsDBNull(col))
            {
                return defaultValue;
            }

            return dr.GetString(col);
        }

        public static Boolean GetNullableBoolean(this SqlDataReader dr, int col, Boolean defaultValue = false)
        {
            if (dr.IsDBNull(col))
            {
                return defaultValue;
            }

            return dr.GetBoolean(col);
        }

        public static Guid GetNullableGuid(this SqlDataReader dr, int col)
        {
            if (dr.IsDBNull(col))
            {
                return Guid.Empty;
            }

            return dr.GetGuid(col);
        }

        public static TimeSpan? GetNullableTimeSpan(this SqlDataReader dr, int col)
        {
            if (dr.IsDBNull(col))
            {
                return null;
            }

            return dr.GetTimeSpan(col);
        }
    }
}
