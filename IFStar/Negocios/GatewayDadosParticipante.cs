using IFStar.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IFStar.Negocios
{
    public class GatewayDadosParticipante
    {
        Conexao conexao = new Conexao();

        public string AddParticipante(Participante participante)
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
                //Verifica se já foi cadastrada a votação. Participantes só serão cadastrados após o cadastro da votação
                DataTable dtDados = GatewayDadosVotacao.VerificarVotacao(DateTime.Now.Year, conn);
                if (dtDados.Rows.Count == 0)
                    throw new Exception("Participantes só serão cadastrados após o cadastro da Votação.");

                bool votacaoAberta = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());
                if (votacaoAberta)
                    throw new Exception("Não podem ser cadastrados participantes com a Votação em andamento.");

                bool votacaoEncerrada = Boolean.Parse(dtDados.Rows[0]["flVotacaoEncerrada"].ToString());
                if (votacaoEncerrada)
                    throw new Exception("Não podem ser cadastrados participantes após o encerramento da Votação.");

                string idParticipante = ConsultaIdParticipante(DateTime.Now.Year.ToString(), conn);
                dsErro = InserirParticipante(idParticipante, participante, conn);
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

        public RetornoParticipanteVoto GetParticipantes()
        {
            RetornoParticipanteVoto retorno = new RetornoParticipanteVoto();
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
                try
                {
                    dtDados = GatewayDadosVotacao.VerificarVotacao(DateTime.Now.Year, conn); //Primeiro consulta a votação
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    bool flVotacaoAberta = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());
                    if (!flVotacaoAberta)
                        throw new Exception("Votação não está disponível. Aguarde o início da votação.");

                    retorno.temaVotacao = dtDados.Rows[0]["tema"].ToString();
                }
                else
                {
                    throw new Exception("Nenhuma votação cadastrada");
                }

                try
                {
                    dtDados = ConsultarParticipantesVoto(DateTime.Now.Year.ToString(), conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados [2]: " + ex.Message);
                }

                if (dtDados.Rows.Count > 0)
                {
                    retorno.qtdParticipantes = dtDados.Rows.Count;
                    retorno.dadosParticipante = new List<DadosParticipante>();

                    for (int i = 0; i < dtDados.Rows.Count; i++)
                    {
                        DadosParticipante participante = new DadosParticipante();
                        participante.idParticipante = dtDados.Rows[i]["idParticipante"].ToString();
                        participante.dsNome = dtDados.Rows[i]["nome"].ToString();
                        participante.dsMusica = dtDados.Rows[i]["musica"].ToString();
                        participante.dsInstEnsino = dtDados.Rows[i]["instEnsino"].ToString();

                        retorno.dadosParticipante.Add(participante);
                    }
                }
                else
                {
                    throw new Exception("Nenhum participante cadastrado para a votação.");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                throw new Exception("Falha ao retornar dados dos Participantes: " + ex.Message);
            }

            conn.Close();
            return retorno;
        }

        #region Métodos

        #region Gerar Id Participante
        public string ConsultaIdParticipante(string ano, SqlConnection conn)
        {
            string idParticipante = string.Empty;
            string anoVotacao = DateTime.Now.Year.ToString();
            
            DataTable dtDados = new DataTable();
            string script = "SELECT COUNT(*) AS qtdParticipantes FROM tbParticipante WHERE SUBSTRING(idParticipante, 1, 4) = @ano";

            //Adicionar parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@ano", ano);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            if (dtDados.Rows.Count == 1)
            {
                string complementoIdParticipante = string.Empty;
                int qtdParticipantes = Int32.Parse(dtDados.Rows[0]["qtdParticipantes"].ToString());

                if (qtdParticipantes <= 9)
                {
                    qtdParticipantes++;
                    complementoIdParticipante = qtdParticipantes == 10 ? "00" + qtdParticipantes : "000" + qtdParticipantes;
                }
                else if (qtdParticipantes >= 10 && qtdParticipantes <= 99)
                {
                    qtdParticipantes++;
                    complementoIdParticipante = qtdParticipantes == 100 ? "0" + qtdParticipantes : "00" + qtdParticipantes;
                }
                else if (qtdParticipantes >= 100 && qtdParticipantes <= 999)
                {
                    qtdParticipantes++;
                    complementoIdParticipante = qtdParticipantes == 1000 ? qtdParticipantes.ToString() : "0" + qtdParticipantes;
                }
                else if (qtdParticipantes >= 1000 && qtdParticipantes <= 9998)
                {
                    qtdParticipantes++;
                    complementoIdParticipante = qtdParticipantes.ToString();
                }
                else
                {
                    throw new Exception("Quantidade de participantes por ano excedida. Contate a Equipe Técnica.");
                }

                idParticipante = anoVotacao + complementoIdParticipante;
            }
            else if (dtDados.Rows.Count > 1)
            {
                throw new Exception("Mais de um resultado encontrado. Contate a Equipe Técnica.");
            }
            else
            {
                throw new Exception("Nenhum resultado encontrado. Contate a Equipe Técnica.");
            }

            return idParticipante;
        }
        #endregion

        #region Inserir Participante
        public string InserirParticipante(string idParticipante, Participante participante, SqlConnection conn)
        {
            string erro = string.Empty;

            string script = "INSERT INTO tbParticipante (idParticipante, nome, musica, instEnsino) VALUES (@idParticipante, @nome, @musica, @instEnsino)";
            if (!string.IsNullOrEmpty(participante.getTelefone()))
                script = "INSERT INTO tbParticipante (idParticipante, nome, musica, instEnsino, telefone) VALUES (@idParticipante, @nome, @musica, @instEnsino, @telefone)";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@idParticipante", idParticipante);
            cmd.Parameters.AddWithValue("@nome", participante.getNome());
            cmd.Parameters.AddWithValue("@musica", participante.getMusica());
            cmd.Parameters.AddWithValue("@instEnsino", participante.getInstEnsino());
            if (!string.IsNullOrEmpty(participante.getTelefone()))
                cmd.Parameters.AddWithValue("@telefone", participante.getTelefone());

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                erro = "Falha ao Inserir Participante: " + ex.Message;
            }

            return erro;
        }
        #endregion

        #region Consultar Participantes
        public DataTable ConsultarParticipantesVoto(string ano, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            string script = "SELECT idParticipante, nome, musica, instEnsino FROM tbParticipante (NOLOCK) WHERE SUBSTRING(idParticipante, 1, 4) = @ano ORDER BY nome";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@ano", ano);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            return dtDados;
        }
        #endregion

        #endregion
    }
}