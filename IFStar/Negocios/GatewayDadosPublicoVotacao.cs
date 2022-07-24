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

        public string AddVoto(ParamVoto voto)
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

            }
            catch (Exception ex)
            {

            }
            #endregion

            return dsErro;
        }

        #region Métodos

        #region Inserir Voto
        public string InserirVoto(ParamVoto voto, SqlConnection conn)
        {
            string dsErro = string.Empty;

            string scriptInsert = "INSERT INTO tbPublicoVotacao (idUsuario, idParticipante, dtVoto, anoVotacao) VALUES (@idUsuario, @idParticipante, @dtVoto, @anoVotacao)";

            //Adicionar parâmetros
            SqlCommand cmd = new SqlCommand(scriptInsert, conn);
            cmd.Parameters.AddWithValue("@idUsuario", voto.idUsuario);
            cmd.Parameters.AddWithValue("@idParticipante", voto.idParticipante);
            cmd.Parameters.AddWithValue("@dtVoto", DateTime.Now.Date);
            cmd.Parameters.AddWithValue("@anoVotacao", DateTime.Now.Year);

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

            string script = "SELECT idUsuario, dtVoto, anoVotacao FROM tbPublicoVotacao WHERE idUsuario = @idUsuario AND anoVotacao = @anoVotacao";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@anoVotacao", DateTime.Now.Year);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            return dtDados;
        }
        #endregion

        #endregion
    }
}