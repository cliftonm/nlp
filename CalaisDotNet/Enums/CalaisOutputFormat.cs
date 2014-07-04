namespace Calais
{
    /// <summary>
    /// Available formats of input content.
    /// </summary>
    public enum CalaisOutputFormat
    {
        [EnumString("xml/rdf")]
        Rdf,
        [EnumString("text/simple")]
        Simple,
        [EnumString("text/microformats")]
        MicroFormats,
        [EnumString("application/json")]
        JSON
    }
}
