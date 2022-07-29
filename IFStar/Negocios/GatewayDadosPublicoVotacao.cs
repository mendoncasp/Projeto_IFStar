using IFStar.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IFStar.Negocios
{
    public class GatewayDadosPublicoVotacao
    {
        Conexao conexao = new Conexao();

        public string AddVoto(PublicoVotacao publicoVotacao)
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
                //Verifica se a votação já está aberta
                DataTable dtDados = new DataTable();
                dtDados = GatewayDadosVotacao.VerificarVotacao(DateTime.Now.Year, conn);

                if (dtDados.Rows.Count == 1)
                {
                    bool flVotacaoAberta = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());
                    if (!flVotacaoAberta)
                        throw new Exception("Voto só pode ser registrado em uma votação aberta.");
                    
                    //Verifica se o usuário já tem voto registrado para aquele ano
                    dtDados = VerificarVoto(publicoVotacao.getIdUsuario(), conn);
                    if (dtDados.Rows.Count == 1)
                    {
                        throw new Exception("Usuário já possui um voto registrado.");
                    }
                    else
                    {
                        dsErro = InserirVoto(publicoVotacao, conn);
                    }
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de uma votação encontrada. Contate a equipe Técnica.");
                }
                else
                {
                    throw new Exception("Nenhuma votação encontrada.");
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

        #region Métodos

        #region Inserir Voto
        public string InserirVoto(PublicoVotacao publicoVotacao, SqlConnection conn)
        {
            string dsErro = string.Empty;

            string scriptInsert = "INSERT INTO tbPublicoVotacao (idUsuario, idParticipante, dtVoto) VALUES (@idUsuario, @idParticipante, @dtVoto)";

            //Adicionar parâmetros
            SqlCommand cmd = new SqlCommand(scriptInsert, conn);
            cmd.Parameters.AddWithValue("@idUsuario", publicoVotacao.getIdUsuario());
            cmd.Parameters.AddWithValue("@idParticipante", publicoVotacao.getIdParticipante());
            cmd.Parameters.AddWithValue("@dtVoto", DateTime.Now.Date);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                dsErro = "Falha ao Registar Voto: " + ex.Message;
            }

            return dsErro;
        }
        #endregion

        #region Verificar Voto
        public DataTable VerificarVoto(int idUsuario, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            string script = "SELECT idUsuario, dtVoto FROM tbPublicoVotacao (NOLOCK) WHERE idUsuario = @idUsuario AND YEAR(dtVoto) = @ano";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@ano", DateTime.Now.Year);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            return dtDados;
        }
        #endregion

        #endregion
    }
}