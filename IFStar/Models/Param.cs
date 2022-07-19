using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFStar.Models
{
    public class Param
    {
        public string dsNome { get; set; }
        public string dsCpf { get; set; }
        public string dsEmail { get; set; }
        public string dsSenha { get; set; }
        public string cdPrivilegio { get; set; }

        #region Métodos
        #region Validar Param
        public void ValidarParam(Param parametro)
        {
            #region Validação Dados
            if (string.IsNullOrEmpty(parametro.dsNome))
                throw new Exception("É Obrigatório informar o Nome do usuário.");

            if (!string.IsNullOrEmpty(parametro.dsCpf))
            {
                if (parametro.dsCpf.Length != 14)
                    throw new Exception("CPF informado está em um formato inválido.");

                if (IsCpf(parametro.dsCpf))
                    this.dsCpf = parametro.dsCpf.Replace(".", "").Replace("-", "");
                else
                    throw new Exception("CPF informado é inválido.");
            }

            if (!string.IsNullOrEmpty(parametro.dsEmail))
            {
                if (IsValidEmail(parametro.dsEmail.Trim()))
                    this.dsEmail = parametro.dsEmail.Trim();
                else
                    throw new Exception("Endereço de E-mail informado de forma incorreta: " + parametro.dsEmail);
            }
            else
            {
                throw new Exception("Deve ser informado um Endereço de E-mail para fazer o cadastro.");
            }

            if (!string.IsNullOrEmpty(parametro.dsSenha))
                this.dsSenha = parametro.dsSenha.Trim();
            else
                throw new Exception("Deve ser informada uma senha de acesso.");
            #endregion
        }
        #endregion

        #region Validação CPF
        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            cpf = cpf.Trim().Replace(".", "").Replace("-", "");
            if (cpf.Length != 11 || cpf == "00000000000")
                return false;

            for (int j = 0; j < 10; j++)
                if (j.ToString().PadLeft(11, char.Parse(j.ToString())) == cpf)
                    return false;

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }
        #endregion

        #region Validação E-mail
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                string host = addr.Host;
                if (host.StartsWith("."))
                    return false;
                if (host.EndsWith("."))
                    return false;
                if (!(host.Contains(".")))
                    return false;
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #endregion
    }

    public class ParamAlteracao
    {
        public int idUsuario { get; set; }
        public string dsSenha { get; set; }
        public string dsNovaSenha { get; set; }
    }

    public class ParamParticipante
    {
        public string dsNome { get; set; }
        public string dsMusica { get; set; }
        public string dsInstEnsino { get; set; }
        public string dsTelefone { get; set; }
    }

    public class ParamVotacao
    {
        public int idVotacao { get; set; }
        public string dsTema { get; set; }
        public int nrEdicao { get; set; }
        public int nrAno { get; set; }
        public DateTime dtVotacao { get; set; }
        public string horaInicio { get; set; }
        public string horaFim { get; set; }
        public int idUserInsert { get; set; }

        #region Métodos
        #region Validar Param
        public void ValidarParamVotacao(ParamVotacao parametro)
        {
            if (string.IsNullOrEmpty(parametro.dsTema))
                throw new Exception("É Obrigatório informar o Tema do evento.");

            if (parametro.nrEdicao < 1)
                throw new Exception("Deve ser informado um número de Edição maior ou igual a 1.");

            int anoAtual = DateTime.Now.Year;
            if (parametro.nrAno < anoAtual || parametro.nrAno > anoAtual)
                throw new Exception("O ano do evento deve igual ao ano atual.");

            if (parametro.dtVotacao.Date < DateTime.Now.Date)
            {
                throw new Exception("A data de iníco deve ser igual ou maior que a data atual.");
            }
            else
            {
                if (parametro.dtVotacao.Year > anoAtual)
                    throw new Exception("O ano do evento deve ser igual ao ano atual [2].");
            }

            string[] horarioInicio = parametro.horaInicio.Trim().Split(':'); //Primeira posição: Horas; Segunda posição: Minutos
            string[] horarioFim = parametro.horaFim.Trim().Split(':'); //Primeira posição: Horas; Segunda posição: Minutos

            if (parametro.dtVotacao.Date == DateTime.Now.Date)
            {
                if (Int32.Parse(horarioInicio[0]) < DateTime.Now.Hour)
                {
                    throw new Exception("O horário de início deve ser maior ou igual ao horário atual.");
                }
                else if (Int32.Parse(horarioInicio[0]) == DateTime.Now.Hour)
                {
                    if (Int32.Parse(horarioInicio[1]) <= DateTime.Now.Minute)
                        throw new Exception("A votação deve ser cadastrada com pelo menos 10 minutos a mais em relação ao horário atual.");
                }
            }

            int periodoVotacao = Int32.Parse(horarioFim[0]) - Int32.Parse(horarioInicio[0]);
            if (periodoVotacao <= 5)
                throw new Exception("Votação deve ter um período mínimo de 6 horas.");

            if (parametro.idUserInsert == 0)
                throw new Exception("Deve ser informado o usuário que está cadastrando a votação.");
        }
        #endregion
        #endregion
    }
}