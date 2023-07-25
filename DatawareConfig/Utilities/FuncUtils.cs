namespace DatawareConfig.Utilities
{
    public static class FuncUtils
    {
        public static string Reemplazar(string? valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                string llave = valor.Replace("{", "").Replace("}", "");
                string corchete = llave.Replace("[", "").Replace("]", "");
                string comilla = corchete.Replace("\"", "");
                string t = comilla.Replace("\t", "");
                string n = t.Replace("\n", "");
                string r = n.Replace("\r", "");
                string trim = r.Trim();
                return trim;
            }
            else
            {
                return "";
            }
            
        }
    }
}
