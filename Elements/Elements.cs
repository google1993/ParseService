using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParseService.Elements
{
    class Process
    {

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

        public string Splav { get; set; }
        public string Ukazanie { get; set; }
        public string Contract { get; set; }

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
        public string Operation { get; set; }
        public bool OperFlag { get; set; }

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

        public DateTime? Prostoy { get; set; }
        [NotMapped]
        public string ProstoyStr
        {
            set
            {
                if (value == "-")
                {
                    Prostoy = null;
                    return;
                }
                int.TryParse(value.Substring(0, value.IndexOf(':')), out int hour);
                int.TryParse(value.Substring(value.IndexOf(':') + 1), out int min);
                TimeSpan t = new TimeSpan(hour, min, 0);
                Prostoy = DateTime.Now.Subtract(t);
            }
        }

        public void ChangeParam(Process b)
        {
            this.PrefixPlav = b.PrefixPlav;
            this.NomerPlav = b.NomerPlav;
            this.Pech = b.Pech;
            this.Splav = b.Splav;
            this.Ukazanie = b.Ukazanie;
            this.Contract = b.Contract;
            this.NaborOg = b.NaborOg;
            this.NaborPer = b.NaborPer;
            this.ComplectNum = b.ComplectNum;
            this.ComplectIzl = b.ComplectIzl;
            this.ComplectDiam = b.ComplectDiam;
            this.ElectVes = b.ElectVes;
            this.Operation = b.Operation;
            this.OperFlag = b.OperFlag;
            this.Start = b.Start;
            this.EndTeor = b.EndTeor;
            this.EndFact = b.EndFact;
            this.Prostoy = b.Prostoy;
        }
    }
}
