using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParseServiceNC2.DB
{
    class PechStatus
    {
        [Key]
        public int ProcessId { get; set; }
        public int? PrefixPlav { get; set; }
        public int? NomerPlav { get; set; }
        public int Pech { get; set; }
        [NotMapped]
        public string Plavka
        {
            get
            {
                if (!PrefixPlav.HasValue || !NomerPlav.HasValue)
                    return Pech.ToString("D2");
                else
                {
                    int nomer = (int)NomerPlav;
                    return PrefixPlav.ToString() + "-" + Pech.ToString("D2") + "-" + nomer.ToString("D5");
                }
            }
            set
            {
                if (int.TryParse(value, out int param))
                {
                    Pech = param;
                    return;
                }
                if (value.Length <= 6)
                {
                    int.TryParse(value.Substring(2, 2), out param);
                    Pech = param;
                    return;
                }
                if (int.TryParse(value.Substring(0, 1), out param))
                    PrefixPlav = param;
                else
                    PrefixPlav = null;
                Pech = int.TryParse(value.Substring(2, 2), out param) ? param : 0;
                if (int.TryParse(value.Substring(5, 5), out param))
                    NomerPlav = param;
                else
                    NomerPlav = null;
            }
        }
        public int? SplavId { get; set; }
        public virtual Splavy Splav { get; set; }
        public int? UkazanieId { get; set; }
        public virtual Ukazaniya Ukazanie { get; set; }
        public int? ContractId { get; set; }
        public virtual Contracts Contract { get; set; }

        public int NaborOg { get; set; }
        public int NaborPer { get; set; }
        [NotMapped]
        public string Nabor
        {
            get
            {
                return NaborOg.ToString() + "-" + NaborPer.ToString();
            }
            set
            {
                if (int.TryParse(value.Substring(0, value.IndexOf('-')), out int param))
                    NaborOg = param;
                if (int.TryParse(value.Substring(value.IndexOf('-') + 1), out param))
                    NaborPer = param;
            }
        }

        public int ComplectNum { get; set; }
        public int ComplectIzl { get; set; }
        public int ComplectDiam { get; set; }
        [NotMapped]
        public string Complect
        {
            get
            {
                return ComplectNum.ToString() + "/" + ComplectIzl.ToString() + "-" + ComplectDiam.ToString();
            }
            set
            {
                if (int.TryParse(value.Substring(0, value.IndexOf('/')), out int param))
                    ComplectNum = param;
                if (int.TryParse(value.Substring(value.IndexOf('/') + 1, value.IndexOf('-') - (value.IndexOf('/') + 1)), out param))
                    ComplectIzl = param;
                if (int.TryParse(value.Substring(value.IndexOf('-') + 1), out param))
                    ComplectDiam = param;
            }
        }

        public int ElectVes { get; set; }
        public int OperationId { get; set; }
        public virtual Operations Operation { get; set; }
        public DateTime Start { get; set; }

        public DateTime? EndTeor { get; set; }
        [NotMapped]
        public string EndTeorStr
        {
            set
            {
                if (value == "-") {
                    EndTeor = null;
                    return;
                }
                int.TryParse(value.Substring(0, value.IndexOf(':')), out int hour);
                int.TryParse(value.Substring(value.IndexOf(':') + 1), out int min);
                TimeSpan t = new TimeSpan(hour, min, 0);
                EndTeor = DateTime.Now.Add(t);

            }
        }

        public DateTime? EndFact { get; set; }
    }
    class Contracts {
        [Key]
        public int ContractId { get; set; }
        public string Contract { get; set; }
        public virtual ICollection<PechStatus> PechStatuses { get; set; }
        public Contracts()
        {
            PechStatuses = new List<PechStatus>();
        }
        public Contracts(string a)
        {
            PechStatuses = new List<PechStatus>();
            Contract = a;
        }

    }
    class Operations {
        [Key]
        public int OperationId { get; set; }
        public string Operation { get; set; }
        public virtual ICollection<PechStatus> PechStatuses { get; set; }
        public Operations()
        {
            PechStatuses = new List<PechStatus>();
        }
        public Operations(string a)
        {
            PechStatuses = new List<PechStatus>();
            Operation = a;
        }

    }
    class Splavy {
        [Key]
        public int SplavId { get; set; }
        public string Splav { get; set; }
        public virtual ICollection<PechStatus> PechStatuses { get; set; }
        public Splavy()
        {
            PechStatuses = new List<PechStatus>();
        }
        public Splavy(string a)
        {
            PechStatuses = new List<PechStatus>();
            Splav = a;
        }
    }
    class Ukazaniya {
        [Key]
        public int UkazanieId { get; set; }
        public string Ukazanie { get; set; }
        public virtual ICollection<PechStatus> PechStatuses { get; set; }
        public Ukazaniya()
        {
            PechStatuses = new List<PechStatus>();
        }
        public Ukazaniya(string a)
        {
            PechStatuses = new List<PechStatus>();
            Ukazanie = a;
        }

    }
}

	
	
	