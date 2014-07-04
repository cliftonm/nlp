namespace Calais
{
    /// <summary>
    /// Available formats of input content
    /// </summary>
    public enum CalaisInputFormat
    {
        [EnumString("text/xml")]
        Xml,
        [EnumString("text/txt")]
        Text,
        [EnumString("text/html")]
        Html,
        [EnumString("text/raw")]
        RawText
    }
}