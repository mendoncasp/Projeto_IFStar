using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class Retorno
    {
        public int idUsuario { get; set; }
        public string dsEmail { get; set; }
        public int cdPrivilegio { get; set; }
    }

    public class RetornoVotacao
    {
        public int idVotacao { get; set; }
        public string dsTema { get; set; }
        public int dsAno { get; set; }
        public DateTime dtVotacao { get; set; }
        public string dsHoraInicio { get; set; }
        public string dsHoraFim { get; set; }
        public bool flAberto { get; set; }
        public bool flVotacaoEncerrada { get; set; }
    }
}