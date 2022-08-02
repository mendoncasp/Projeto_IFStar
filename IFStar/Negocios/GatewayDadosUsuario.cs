using IFStar.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace IFStar.Negocios
{
    public class GatewayDadosUsuario
    {
        #region Campos
        public enum Campos
        {
            Email = 100,
            Senha = 101,
            CPF = 102,
            TipoServico = 103,
            IdUsuario = 104,
            NovaSenha = 105
        }
        #endregion

        Conexao conexao = new Conexao();
        //SqlCommand cmd = new SqlCommand();
        //string connectionString = @"Data Source=DESKTOP-QBCN7D6\SQLEXPRESS;Initial Catalog=dbIFStar;Integrated Security=True";

        public string AddUsuario(Usuario usuario)
        {
            string dsErro = string.Empty;
            SqlConnection conn = new SqlConnection(conexao.connectionString);

            #region Inserir Dados
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao estabelecer conexão com o Banco de Dados: " + ex.Message);
            }

            try
            {
                //Verifica se usuário já existe
                DataTable dadosUsuario = VerificaUsuario(usuario.getCpf(), usuario.getEmail(), conn);
                if (dadosUsuario.Rows.Count > 0)
                    throw new Exception("Usuário já possui cadastro no Sistema.");

                if (!String.IsNullOrEmpty(usuario.getCpf()) && usuario.getCdPrivilegio() > 1)
                {
                    dsErro = InserirAdministrador(usuario, conn);
                }
                else if (String.IsNullOrEmpty(usuario.getCpf()) && usuario.getCdPrivilegio() == 1)
                {
                    dsErro = InserirPublico(usuario, conn);
                }
                else
                {
                    dsErro = "Esse tipo de usuário exige que informe o CPF e o Privilégio.";
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                dsErro = ex.Message;
            }
            #endregion

            conn.Close();
            return dsErro;
        }

        public Retorno GetUsuario(Dictionary<string, string> queryString)
        {
            Retorno retorno = new Retorno();
            string outValue = null;
            DataTable dtDados = new DataTable();
            SqlConnection conn = new SqlConnection(conexao.connectionString);

            //Preenchimento das variáveis com os parâmetros
            string cpf = string.Empty;
            if (queryString.TryGetValue("" + (int)Campos.CPF, out outValue))
            {
                cpf = queryString["" + (int)Campos.CPF];
                if (!Param.IsCpf(cpf))
                    throw new Exception("CPF informado é inválido.");
            }

            string email = string.Empty;
            if (queryString.TryGetValue("" + (int)Campos.Email, out outValue))
            {
                email = queryString["" + (int)Campos.Email];
                if (!Param.IsValidEmail(email))
                    throw new Exception("E-mail informado é inválido.");
            }

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao estabelecer conexão com o Banco de Dados: " + ex.Message);
            }

            try
            {
                if ((!string.IsNullOrEmpty(cpf) && !string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(email)))
                    throw new Exception("Quantidade de Parâmetros Inválida! Informar o CPF ou E-mail para consulta.");

                string senhaInformada;
                if (queryString.TryGetValue("" + (int)Campos.Senha, out outValue))
                    senhaInformada = queryString["" + (int)Campos.Senha];
                else
                    throw new Exception("Senha de acesso deve ser informada.");

                try
                {
                    dtDados = VerificaUsuario(cpf, email, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    string senhaUsuario = dtDados.Rows[0]["senha"].ToString();
                    senhaInformada = DecodeFrom64(senhaInformada);
                    senhaUsuario = DecodeFrom64(senhaUsuario);

                    if (senhaUsuario.Equals(senhaInformada))
                    {
                        retorno.idUsuario = Int32.Parse(dtDados.Rows[0]["idUsuario"].ToString());
                        retorno.dsEmail = dtDados.Rows[0]["email"].ToString();
                        retorno.cdPrivilegio = Int32.Parse(dtDados.Rows[0]["cdPrivilegio"].ToString());
                    }
                    else
                    {
                        throw new Exception("Senha Incorreta!");
                    }
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de um usuário encontrado. Contate a Equipe Técnica.");
                }
                else
                {
                    throw new Exception("E-mail ou CPF incorreto!");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                if (ex.Message.ToLower().Contains("senha") || ex.Message.ToLower().Contains("incorreto"))
                    throw new Exception("Falha ao Realizar Acesso: " + ex.Message);
                else
                    throw new Exception("Falha no processamento de dados do Usuário: " + ex.Message);
            }

            conn.Close();
            return retorno;
        }

        public string RecuperarSenha(Dictionary<string, string> queryString)
        {
            string dsErro = string.Empty;
            string outValue = null;
            DataTable dtDados = new DataTable();
            SqlConnection conn = new SqlConnection(conexao.connectionString);
            SqlTransaction tran = null;

            //Preenchimento das variáveis com os parâmetros
            string cpf = string.Empty;
            if (queryString.TryGetValue("" + (int)Campos.CPF, out outValue))
            {
                cpf = queryString["" + (int)Campos.CPF];
                if (!Param.IsCpf(cpf))
                    throw new Exception("CPF informado é inválido.");
            }

            string email = string.Empty;
            if (queryString.TryGetValue("" + (int)Campos.Email, out outValue))
            {
                email = queryString["" + (int)Campos.Email];
                if (!Param.IsValidEmail(email))
                    throw new Exception("E-mail informado é inválido.");
            }

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao estabelecer conexão com o Banco de Dados: " + ex.Message);
            }

            try
            {
                try
                {
                    dtDados = VerificaUsuario(cpf, email, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    StringBuilder sb = new StringBuilder();
                    Random rnd = new Random();
                    int cont = 8;
                    string Strings = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%&0123456789";

                    for (int i = 0; i <= cont; i++)
                    {
                        sb.Append(Strings[rnd.Next(0, Strings.Length)]);
                    }

                    string senhaUsuario = sb.ToString(); //Senha que será enviada por e-mail
                    string novaSenha = EncodeFrom64(sb.ToString()); //Senha que será registrada no Banco de Dados
                    int idUsuario = Int32.Parse(dtDados.Rows[0]["idUsuario"].ToString());
                    string emailUsuario = dtDados.Rows[0]["email"].ToString();

                    tran = conn.BeginTransaction();
                    string scriptUpdate = "UPDATE tbUsuario SET senha = @senha WHERE idUsuario = @idUsuario";

                    //Adicionar Parâmetros
                    SqlCommand cmd = new SqlCommand(scriptUpdate, conn);
                    cmd.Transaction = tran;
                    cmd.Parameters.AddWithValue("@senha", novaSenha);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                    cmd.ExecuteNonQuery();

                    StringBuilder emailRecuperacao = new StringBuilder();
                    emailRecuperacao.AppendLine("Prezado Usuário,");
                    emailRecuperacao.AppendLine();
                    emailRecuperacao.AppendLine("Conforme solicitado, segue nova senha que será utilizada para acessar o sistema de votação da Final do Festival IFStar.");
                    emailRecuperacao.AppendLine();
                    emailRecuperacao.AppendLine("Senha não pode ser alteradao. Portanto, guardar as informações desse e-mail para acessos futuros.");
                    emailRecuperacao.AppendLine();
                    emailRecuperacao.AppendLine(!string.IsNullOrEmpty(cpf) ? "CPF do Usuário: " + cpf : "E-mail do Usuário: " + email);
                    emailRecuperacao.AppendLine("Nova Senha: " + senhaUsuario);

                    Mail.Send("Recuperação de Senha - Festival IFStar", emailRecuperacao.ToString(), emailUsuario);

                    tran.Commit();
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de um usuário encontrado. Contate a equipe Técnica [2].");
                }
                else
                {
                    throw new Exception("E-mail ou CPF incorreto [2]!");
                }
            }
            catch (Exception ex)
            {
                if (tran != null)
                    tran.Rollback();

                if (conn != null)
                    conn.Close();

                dsErro = "Falha na alteração de senha do Usuário: " + ex.Message;
            }

            conn.Close();
            return dsErro;
        }

        public string AlterUsuario(ParamAlteracao param)
        {
            string dsErro = string.Empty;
            SqlConnection conn = new SqlConnection(conexao.connectionString);
            DataTable dtDados = new DataTable();
            int idUsuario = 0;

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao estabelecer conexão com o Banco de Dados: " + ex.Message);
            }

            idUsuario = param.idUsuario;
            if (idUsuario == 0)
                throw new Exception("Deve ser informado o número do Usuário para consulta. Contate a equipe Técnica");

            try
            {
                try
                {
                    //Consultar dados do usuário
                    dtDados = ConsultarUsuario(idUsuario, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    string senhaUsuario = dtDados.Rows[0]["senha"].ToString();
                    senhaUsuario = DecodeFrom64(senhaUsuario);

                    string senhaInformada = param.dsSenha;
                    senhaInformada = DecodeFrom64(senhaInformada);
                    if (senhaUsuario.Equals(senhaInformada))
                    {
                        string novaSenha = param.dsNovaSenha;
                        string scriptUpdate = "UPDATE tbUsuario SET senha = @senha WHERE idUsuario = @idUsuario";

                        //Adicionar Parâmetros
                        SqlCommand cmd = new SqlCommand(scriptUpdate, conn);
                        cmd.Parameters.AddWithValue("@senha", novaSenha);
                        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        throw new Exception("Senha incorreta");
                    }
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de um usuário encontrado. Contate a equipe Técnica.");
                }
                else
                {
                    throw new Exception("Nenhum usuário encontrado.");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                dsErro = "Falha na alteração de senha: " + ex.Message;
            }

            conn.Close();
            return dsErro;
        }

        #region Métodos

        #region Inserir Administrador
        public static string InserirAdministrador(Usuario usuario, SqlConnection conn)
        {
            string erro = string.Empty;

            string scriptInsert = "INSERT INTO tbUsuario (nome, cpf, email, senha, cdPrivilegio) VALUES (@nome, @cpf, @email, @senha, @cdPrivilegio)";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(scriptInsert, conn);
            cmd.Parameters.AddWithValue("@nome", usuario.getNome());
            cmd.Parameters.AddWithValue("@cpf", usuario.getCpf());
            cmd.Parameters.AddWithValue("@email", usuario.getEmail());
            cmd.Parameters.AddWithValue("@senha", usuario.getSenha());
            cmd.Parameters.AddWithValue("@cdPrivilegio", usuario.getCdPrivilegio());

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                erro = "Falha ao Inserir registro no Banco de Dados: " + ex.Message;
            }

            return erro;
        }
        #endregion

        #region Inserir Público
        public static string InserirPublico(Usuario usuario, SqlConnection conn)
        {
            string erro = String.Empty;

            string scriptInsert = "INSERT INTO tbUsuario (nome, email, senha) VALUES (@nome, @email, @senha)";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(scriptInsert, conn);
            cmd.Parameters.AddWithValue("@nome", usuario.getNome());
            cmd.Parameters.AddWithValue("@email", usuario.getEmail());
            cmd.Parameters.AddWithValue("@senha", usuario.getSenha());

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                erro = "Falha ao Inserir registro no Banco de Dados: " + ex.Message;
            }

            return erro;
        }
        #endregion

        #region Verificar Usuário
        public static DataTable VerificaUsuario(string cpf, string email, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            if (!string.IsNullOrEmpty(cpf)) //Busca primeiro pelo CPF, que não é campo obrigatório
            {
                string script = "SELECT idUsuario, email, senha, cdPrivilegio FROM tbUsuario (NOLOCK) WHERE cpf LIKE @cpf";

                //Adicionar Parâmetros
                SqlCommand cmd = new SqlCommand(script, conn);
                cmd.Parameters.AddWithValue("@cpf", cpf);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtDados);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                string script = "SELECT idUsuario, email, senha, cdPrivilegio FROM tbUsuario (NOLOCK) WHERE email LIKE @email";

                //Adicionar Parâmetros
                SqlCommand cmd = new SqlCommand(script, conn);
                cmd.Parameters.AddWithValue("@email", email);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtDados);
            }
            else
            {
                throw new Exception("Falha ao Consultar Usuário: Faltam parâmetros para realizar a consulta.");
            }

            return dtDados;
        }
        #endregion

        #region Consultar Usuário
        public static DataTable ConsultarUsuario(int idUsuario, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            string script = "SELECT idUsuario, email, senha, cdPrivilegio FROM tbUsuario (NOLOCK) WHERE idUsuario = @idUsuario";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("idUsuario", idUsuario);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            return dtDados;
        }
        #endregion

        #region Decodificar Senha
        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes =
                System.Convert.FromBase64String(encodedData);
            string returnValue =
                System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
        #endregion

        #region Codificar Senha
        public static string EncodeFrom64(string toEncode)
        {
            byte[] toEncodeAsBytes =
                System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue =
                System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        #endregion

        #endregion
    }
}