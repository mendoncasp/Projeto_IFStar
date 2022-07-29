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

    public class DadosParticipante
    {
        public string idParticipante { get; set; }
        public string dsNome { get; set; }
        public string dsMusica { get; set; }
        public string dsInstEnsino { get; set; }
    }

    public class RetornoParticipanteVoto
    {
        public string temaVotacao { get; set; }
        public int qtdParticipantes { get; set; }
        public List<DadosParticipante> dadosParticipante { get; set; }
    }
}