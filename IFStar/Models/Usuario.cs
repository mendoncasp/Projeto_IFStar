using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class Usuario
    {
        public Usuario(string nome, string cpf, string email, string senha, int cdPrivilegio)
        {
            this.Nome = nome;
            this.CPF = cpf;
            this.email = email;
            this.senha = senha;
            this.cdPrivilegio = cdPrivilegio;
        }

        private string Nome { get; set; }
        private string CPF { get; set; }
        private string email { get; set; }
        private string senha { get; set; }
        private int cdPrivilegio { get; set; }

        public string getNome()
        {
            return Nome;
        }

        public void setNome(string nome)
        {
            this.Nome = nome;
        }

        public string getCpf()
        {
            return CPF;
        }

        public void setCpf(string cpf)
        {
            this.CPF = cpf;
        }

        public string getEmail()
        {
            return email;
        }

        public void setEmail(string email)
        {
            this.email = email;
        }

        public string getSenha()
        {
            return senha;
        }

        public void setSenha(string senha)
        {
            this.senha = senha;
        }

        public int getCdPrivilegio()
        {
            return cdPrivilegio;
        }

        public void setCdPrivilegio(int cdPrivilegio)
        {
            this.cdPrivilegio = cdPrivilegio;
        }
    }
}