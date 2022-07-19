using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class Votacao
    {
        public Votacao(string tema, int edicao, int ano, DateTime dtVotacao, string horaInicio, string horaFim, int idUserInsert)
        {
            this.tema = tema;
            this.edicao = edicao;
            this.ano = ano;
            this.dtVotacao = dtVotacao;
            this.horaInicio = horaInicio;
            this.horaFim = horaFim;
            this.idUserInsert = idUserInsert;
        }

        private string tema { get; set; }
        private int edicao { get; set; }
        private int ano { get; set; }
        private DateTime dtVotacao { get; set; }
        private string horaInicio { get; set; }
        private string horaFim { get; set; }
        private int idUserInsert { get; set; }

        public string getTema()
        {
            return tema;
        }

        public void setTema(string tema)
        {
            this.tema = tema;
        }

        public int getEdicao()
        {
            return edicao;
        }

        public void setEdicao(int edicao)
        {
            this.edicao = edicao;
        }

        public int getAno()
        {
            return ano;
        }

        public void setAno(int ano)
        {
            this.ano = ano;
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

        public int getIdUserInsert()
        {
            return idUserInsert;
        }

        public void setIdUserInsert(int idUserInsert)
        {
            this.idUserInsert = idUserInsert;
        }
    }
}