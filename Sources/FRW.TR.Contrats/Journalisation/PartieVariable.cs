namespace FRW.TR.Contrats.Journalisation
{
    public class PartieVariable<T>
    {
        public PartieVariable(string code, T valeur, TypePartieVariable type = TypePartieVariable.E)
        {
            Type = type;
            Code = code;
            Valeur = valeur;
        }

        /// <summary>
        /// Type de la partie variable; Valeur par défaut à "Entité" pour l'ensemble d'ECS.
        /// </summary>
        public TypePartieVariable Type { get; set; } = TypePartieVariable.E;

        /// <summary>
        /// Code de la partie variable.
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Valeur de la partie variable.
        /// </summary>
        public T Valeur { get; set; }
    }
}
