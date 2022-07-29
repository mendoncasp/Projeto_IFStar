using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class PublicoVotacao
    {
        public PublicoVotacao(int idUsuario, string idParticipante)
        {
            this.idUsuario = idUsuario;
            this.idParticipante = idParticipante;
        }

        private int idUsuario { get; set; }
        private string idParticipante { get; set; }

        public int getIdUsuario()
        {
            return idUsuario;
        }

        public void setIdUsuario(int idUsuario)
        {
            this.idUsuario = idUsuario;
        }

        public string getIdParticipante()
        {
            return idParticipante;
        }

        public void setIdParticipante(string idParticipante)
        {
            this.idParticipante = idParticipante;
        }
    }
}