namespace Calais
{
    /// <summary>
    /// A few handy string extension methods
    /// </summary>
    public static class StringLibrary
    {
        public static string ToPascal(this string value)
        {
            var s = value;
            s = s.Trim().ToLower();

            if (s.Length == 0)
            {
                return value;
            }
            string[] ar = null;

            if (s.IndexOf('_') > -1)
            {
                ar = s.Split('_');
            }
            else if (s.IndexOf(' ') > -1)
            {
                ar = s.Split(' ');
            }
            s = string.Empty;
            if (ar != null)
            {
                for (int i = 0; i < ar.Length; i++)
                {
                    if (ar[i].Trim().Length > 0)
                    {
                        s += ar[i].Substring(0, 1).ToUpper() + ar[i].Substring(1);
                    }
                }
                return s;
            }
            return value;
        }


        public static string ToCamel(this string value)
        {
            var s = value;
            s = s.Trim().ToLower();

            if (s.Length == 0)
            {
                return value;
            }
            string[] ar = null;

            if (s.IndexOf('_') > -1)
            {
                ar = s.Split('_');
            }
            else if (s.IndexOf(' ') > -1)
            {
                ar = s.Split(' ');
            }

            if (ar != null)
            {
                for (int i = 0; i < ar.Length; i++)
                {
                    if (ar[i].Trim().Length > 0)
                    {
                        if (i == 0)
                        {
                            s = ar[i];
                        }
                        else
                        {
                            s += ar[i].Substring(0, 1).ToUpper() + ar[i].Substring(1);
                        }
                    }
                }
                return s;
            }
            return value;
        }



        public static string Capitalize(this string value)
        {
            var s = value;
            s = s.Trim().ToLower();

            if (s.Length == 0)
            {
                return value;
            }

            string[] ar = s.Split('.');

            s = string.Empty;
            if (ar != null)
            {
                for (int i = 0; i < ar.Length; i++)
                {
                    if (ar[i].Trim().Length > 0)
                    {
                        s += ar[i].Trim().Substring(0, 1).ToUpper() + ar[i].Trim().Substring(1) + ". ";
                    }
                }
                return s;
            }
            return value;
        }



        public static string CapitalizeAll(this string value)
        {
            var s = value;
            s = s.Trim().ToLower();

            if (s.Length == 0)
            {
                return value;
            }

            string[] ar = s.Split(' ');

            s = string.Empty;
            if (ar != null)
            {
                for (int i = 0; i < ar.Length; i++)
                {
                    if (ar[i].Trim().Length > 0)
                    {
                        s += ar[i].Trim().Substring(0, 1).ToUpper() + ar[i].Trim().Substring(1) + " ";
                    }
                }
                return s.Trim();
            }
            return value;
        }

    }
}