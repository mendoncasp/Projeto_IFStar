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

        public DadosResultado GetResultado()
        {
            DadosResultado dadosResultado = new DadosResultado();
            DataTable dtDados = new DataTable();
            SqlConnection conn = new SqlConnection(conexao.connectionString);

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
                //Verifica se a votação já foi encerrada
                dtDados = GatewayDadosVotacao.VerificarVotacao(DateTime.Now.Year, conn);
                if (dtDados.Rows.Count == 1)
                {
                    bool votacaoEncerrada = Boolean.Parse(dtDados.Rows[0]["flVotacaoEncerrada"].ToString());
                    if (!votacaoEncerrada)
                        throw new Exception("Resultado só será apresentado após o encerramento da Votação.");

                    dadosResultado.temaVotacao = dtDados.Rows[0]["tema"].ToString();

                    //Consulta o resultado
                    dtDados = ConsultarResultado(DateTime.Now.Year, conn);
                    if (dtDados.Rows.Count == 3)
                    {
                        dadosResultado.resultado = new List<Resultado>();
                        int votosEdicao = QuantidadeVotos(DateTime.Now.Year, conn);

                        for (int i = 0; i < dtDados.Rows.Count; i++)
                        {
                            Resultado resultado = new Resultado();
                            resultado.dsNome = dtDados.Rows[i]["nome"].ToString();
                            resultado.dsMusica = dtDados.Rows[i]["musica"].ToString();
                            resultado.dsInstEnsino = dtDados.Rows[i]["instEnsino"].ToString();
                            resultado.qtdVotos = Int32.Parse(dtDados.Rows[i]["qtdVotos"].ToString());
                            int votosParticipante = Int32.Parse(dtDados.Rows[i]["qtdVotos"].ToString());

                            decimal percentualVoto = ((votosParticipante * 100) / votosEdicao);
                            resultado.percVoto = percentualVoto;

                            dadosResultado.resultado.Add(resultado);
                        }
                    }
                    else
                    {
                        throw new Exception("Quantidade de votos retornada inválida.");
                    }
                }
                else
                {
                    throw new Exception("Nenhuma votação encontrada [2].");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                throw new Exception("Falha ao retornar Resultado: " + ex.Message);
            }

            conn.Close();
            return dadosResultado;
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

            if (dtDados.Rows.Count > 1)
            {
                throw new Exception("Mais de um voto encontrada. Contate a equipe Técnica.");
            }

            return dtDados;
        }
        #endregion

        #region Consultar Resultado
        public DataTable ConsultarResultado(int ano, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            string script = "SELECT TOP 3 P.nome, P.musica, P.instEnsino, COUNT(*) AS qtdVotos FROM tbPublicoVotacao PV (NOLOCK) JOIN tbParticipante P (NOLOCK) ON P.idParticipante = PV.idParticipante WHERE YEAR(dtVoto) = @ano GROUP BY P.nome, P.musica, P.instEnsino ORDER BY qtdVotos DESC";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@ano", ano);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            if (dtDados.Rows.Count == 0)
            {
                throw new Exception("A consulta não trouxe nenhum participante. Contate a Equipe Técnica.");
            }

            return dtDados;
        }
        #endregion

        #region Quantidade Votos
        public int QuantidadeVotos(int ano, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();
            int qtdVotos = 0;

            string script = "SELECT COUNT(*) AS qtdVoto FROM tbPublicoVotacao (NOLOCK) WHERE YEAR(dtVoto) = @ano";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@ano", ano);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            qtdVotos = Int32.Parse(dtDados.Rows[0]["qtdVoto"].ToString());

            if (qtdVotos == 0)
                throw new Exception("Quantidade de votos inválida.");

            return qtdVotos;
        }
        #endregion

        #endregion
    }
}