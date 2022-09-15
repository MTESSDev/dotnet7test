using MessagePack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace FRW.TR.Contrats.Contexte
{
#pragma warning disable CS8618 // Un champ non-nullable doit contenir une valeur non-null lors de la fermeture du constructeur. Envisagez de déclarer le champ comme nullable.
    [DataContract]
    public class ContexteCAC : IContexteCAC
    {
        private DateTime _dateHeureProduction = DateTime.MinValue;
        private string _dateReference = "9999-99-99";

        [DataMember]
        public string CodeSysteme { get; set; }

        [DataMember]
        public string CodeUtilisateur { get; set; }

        [DataMember]
        public string CodeUtilisateurComplet { get; set; }

        [DataMember]
        public string CodeUtilisateurCompletUPN { get; set; }

        [DataMember]
        public string Domaine { get; set; }

        [DataMember]
        public string DomaineUPN { get; set; }

        [DataMember]
        public string Phase { get; set; }

        [DataMember]
        public string Environnement { get; set; }

        [DataMember]
        public string Palier { get; set; }

        [IgnoreMember]
        public DateTime DateProduction => DateHeureProduction.Date;

        [IgnoreMember]
        public DateTime DateHeureProduction
        {
            get
            {
                if (_dateHeureProduction == DateTime.MinValue)
                {
                    var dateActuelle = DateTime.Now;
                    _dateHeureProduction = dateActuelle;

                    if (!string.IsNullOrEmpty(DateReference) && !DateReference.Equals("9999-99-99"))
                    {
                        _dateHeureProduction = (DateTime.TryParseExact(DateReference, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _dateHeureProduction)
                            ? _dateHeureProduction.AddHours(dateActuelle.Hour).AddMinutes(dateActuelle.Minute).AddSeconds(dateActuelle.Second).AddMilliseconds(dateActuelle.Millisecond)
                            : dateActuelle);
                    }
                }
                return _dateHeureProduction;
            }
        }

        [DataMember]
        public string DateReference
        {
            get { return _dateReference; }
            set
            {
                _dateReference = value;
                _dateHeureProduction = DateTime.MinValue;
            }
        }

        [DataMember]
        public string Poste { get; set; } = default!;

        [DataMember]
        public bool EstProduction { get; set; }

        [DataMember]
        public Dictionary<string, string>? SurchargesVariablesProfil { get; set; }

        public static ContexteCAC FromContexte(CAC.GestionContexte.Contexte contexte)
        {
            return contexte;
        }

        public static implicit operator ContexteCAC(CAC.GestionContexte.Contexte contexte)
        {
            if (contexte is null)
            {
                return default!;
            }

            return new ContexteCAC()
            {
                CodeSysteme = contexte.CodeSysteme,
                Poste = contexte.Poste,
                Phase = contexte.Phase,
                Palier = contexte.Palier,
                EstProduction = contexte.EstProduction,
                Environnement = contexte.Environnement,
                DomaineUPN = contexte.DomaineUPN,
                CodeUtilisateur = contexte.CodeUtilisateur,
                CodeUtilisateurComplet = contexte.CodeUtilisateurComplet,
                CodeUtilisateurCompletUPN = contexte.CodeUtilisateurCompletUPN,
                Domaine = contexte.Domaine,
                DateReference = contexte.DateProduction.ToString("yyyy-MM-dd")
            };
        }

        public static implicit operator CAC.GestionContexte.Contexte(ContexteCAC contexteCAC)
        {
            if (contexteCAC is null) { return default!; }

            return new CAC.GestionContexte.Contexte(new ResolutionContexteAPI(contexteCAC));
        }

        public CAC.GestionContexte.Contexte ToContexte()
        {
            return new CAC.GestionContexte.Contexte(new ResolutionContexteAPI(this));
        }

        public CAC.GestionContexte.Contexte ToContextePourAppelCentral()
        {
            throw new NotImplementedException();
            /*if (EstProduction)
            {
                //En production il faut tricher et passer le compe de l'APP POOL.
                //Ceci permet de contacter le central et ne pas se soucier que l'utilisateur
                //ait ou non un profil au central (l'app pool ayant assurément accès)
                var utilisateur = GestionUtilisateur.ObtenirInformationsUtilisateurWin32();

                return new CAC.GestionContexte.Contexte(new ResolutionContexteCentral(this,
                                                        utilisateur.CodeUtilisateur,
                                                        utilisateur.DomaineUtilisateur,
                                                        utilisateur.DomaineUtilisateurUPN));
            }
            else
            {
                return ToContexte();
            }*/
        }
    }
#pragma warning restore CS8618 // Un champ non-nullable doit contenir une valeur non-null lors de la fermeture du constructeur. Envisagez de déclarer le champ comme nullable.
}
