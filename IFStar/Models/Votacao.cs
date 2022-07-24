using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class Votacao
    {
        public Votacao(string tema, DateTime dtVotacao, string horaInicio, string horaFim)
        {
            this.tema = tema;
            this.dtVotacao = dtVotacao;
            this.horaInicio = horaInicio;
            this.horaFim = horaFim;
        }

        private string tema { get; set; }
        private DateTime dtVotacao { get; set; }
        private string horaInicio { get; set; }
        private string horaFim { get; set; }

        public string getTema()
        {
            return tema;
        }

        public void setTema(string tema)
        {
            this.tema = tema;
        }

        public DateTime getDtVotacao()
        {
            return dtVotacao;
        }

        public void setDtVotacao(DateTime dtVotacao)
        {
            this.dtVotacao = dtVotacao;
        }

        public string getHoraInicio()
        {
            return horaInicio;
        }

        public void setHoraInicio(string horaInicio)
        {
            this.horaInicio = horaInicio;
        }

        public string getHoraFim()
        {
            return horaFim;
        }

        public void setHoraFim(string horaFim)
        {
            this.horaFim = horaFim;
        }

    }
}