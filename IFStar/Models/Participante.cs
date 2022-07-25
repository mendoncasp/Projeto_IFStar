using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class Participante
    {
        public Participante(string nome, string musica, string instEnsino, string telefone)
        {
            this.nome = nome;
            this.musica = musica;
            this.instEnsino = instEnsino;
            this.telefone = telefone;
        }

        private string nome { get; set; }
        private string musica { get; set; }
        private string instEnsino { get; set; }
        private string telefone { get; set; }

        public string getNome()
        {
            return nome;
        }

        public void setNome(string nome)
        {
            this.nome = nome;
        }

        public string getMusica()
        {
            return musica;
        }

        public void setMusica(string musica)
        {
            this.musica = musica;
        }

        public string getInstEnsino()
        {
            return instEnsino;
        }

        public void setInstEnsino(string instEnsino)
        {
            this.instEnsino = instEnsino;
        }

        public string getTelefone()
        {
            return telefone;
        }

        public void setTelefone(string telefone)
        {
            this.telefone = telefone;
        }
    }
}