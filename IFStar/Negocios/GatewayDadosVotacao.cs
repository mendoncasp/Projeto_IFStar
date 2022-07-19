using IFStar.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IFStar.Negocios
{
    public class GatewayDadosVotacao
    {
        Conexao conexao = new Conexao();

        public string AddVotacao(Votacao votacao)
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
                //Verifica se já existe uma votação para o ano atual. OBS: No momento do desenvolvimento, o evento tem apenas uma edição por ano
                DataTable dadosVotacao = VerificarVotacao(votacao.getAno(), conn);
                if (dadosVotacao.Rows.Count > 0)
                    throw new Exception("Já existe uma Votação cadastrada para esse ano.");

                dsErro = InserirVotacao(votacao, conn);
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

        public string AlterVotacao(ParamVotacao votacao)
        {
            string dsErro = string.Empty;
            SqlConnection conn = new SqlConnection(conexao.connectionString);
            DataTable dtDados = new DataTable();
            int idVotacao = 0;

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao estabelecer conexão com o Banco de Dados: " + ex.Message);
            }

            idVotacao = votacao.idVotacao;
            if (idVotacao == 0)
                throw new Exception("Deve ser informado o número da Votação para consulta. Contate a equipe Técnica.");

            try
            {
                try
                {
                    //Consultar dados da votação
                    dtDados = ConsultarVotacao(idVotacao, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    bool alterarDataVotacao = false;
                    bool alterarDataInicioVotacao = false;
                    bool alterarDataInicioFimVotacao = false;
                    bool alterarDataFimVotacao = false;
                    bool alterarHoraInicioFim = false;
                    bool alterarHoraInicio = false;
                    bool alterarHoraFim = false;
                    List<String> scriptUpdate = new List<String>();

                    DateTime dtVotacaoSistema = DateTime.Parse(dtDados.Rows[0]["dtVotacao"].ToString());
                    string horaInicioSistema = dtDados.Rows[0]["horaInicio"].ToString();
                    string horaFimSistema = dtDados.Rows[0]["horaFim"].ToString();

                    bool votacaoAberta = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());

                    if (!votacaoAberta)
                    {
                        if (DateTime.Now.Date > dtVotacaoSistema.Date)
                            throw new Exception("Não pode ser alterada uma Votação que foi encerrada.");

                        #region Alterar Tema
                        if (!string.IsNullOrEmpty(votacao.dsTema))
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET tema = '" + votacao.dsTema + "' WHERE idVotacao = " + idVotacao);
                        }
                        #endregion

                        #region Alterar Data e Horários
                        string dtVotacaoAtual = votacao.dtVotacao.ToString();
                        if (!dtVotacaoAtual.Contains("01/01/0001") && votacao.dtVotacao.Date != dtVotacaoSistema.Date) //Alterar Data
                        {
                            if (votacao.dtVotacao.Date > dtVotacaoSistema.Date) //Alterar para depois: Livre
                            {
                                alterarDataVotacao = true;

                                if (!string.IsNullOrEmpty(votacao.horaInicio) && !string.IsNullOrEmpty(votacao.horaFim)) //Alterar Hora Abertura e Encerramento
                                {
                                    string[] arrayInicio = votacao.horaInicio.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                    string[] arrayFim = votacao.horaFim.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas.");

                                    alterarDataInicioFimVotacao = true;
                                }
                                else if (!string.IsNullOrEmpty(votacao.horaInicio)) //Alterar Hora Abertura
                                {
                                    string[] arrayInicio = votacao.horaInicio.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                    string[] arrayFim = horaFimSistema.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas. Para alterar o horário de abertura deve ser alterado também o de encerramento.");

                                    alterarDataInicioVotacao = true;
                                }
                                else if (!string.IsNullOrEmpty(votacao.horaFim)) //Alterar Hora Encerramento
                                {
                                    string[] arrayInicio = horaInicioSistema.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                    string[] arrayFim = votacao.horaFim.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas. Para alterar o horário de encerramento deve ser alterado também o de abertura.");

                                    alterarDataFimVotacao = true;
                                }
                            }
                            else if (votacao.dtVotacao.Date < dtVotacaoSistema.Date) //Alterar para antes: Precisa validar horário
                            {
                                if (votacao.dtVotacao.Date == DateTime.Now.Date)
                                {
                                    if (string.IsNullOrEmpty(votacao.horaInicio) || string.IsNullOrEmpty(votacao.horaFim))
                                        throw new Exception("Para alterar a votação para a data atual, é necessário informar o horário de abertura e encerramento");
                                    else
                                    {
                                        string[] arrayInicio = votacao.horaInicio.Split(':');
                                        int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);
                                        if (horaVotacaoInicio <= DateTime.Now.Hour)
                                            throw new Exception("Deve ser informada uma hora maior que a atual.");

                                        string[] arrayFim = votacao.horaFim.Split(':');
                                        int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                        int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                        if (periodoVotacao <= 5)
                                            throw new Exception("Votação deve ter um período mínimo de 6 horas [2].");

                                        alterarDataInicioFimVotacao = true;
                                    }
                                }
                                else
                                {
                                    alterarDataVotacao = true;

                                    if (!string.IsNullOrEmpty(votacao.horaInicio) && !string.IsNullOrEmpty(votacao.horaFim))
                                    {
                                        string[] arrayInicio = votacao.horaInicio.Split(':');
                                        int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                        string[] arrayFim = votacao.horaFim.Split(':');
                                        int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                        int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                        if (periodoVotacao <= 5)
                                            throw new Exception("Votação deve ter um período mínimo de 6 horas [3].");

                                        alterarDataInicioFimVotacao = true;
                                    }
                                    else if (!string.IsNullOrEmpty(votacao.horaInicio))
                                    {
                                        string[] arrayInicio = votacao.horaInicio.Split(':');
                                        int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                        string[] arrayFim = horaFimSistema.Split(':');
                                        int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                        int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                        if (periodoVotacao <= 5)
                                            throw new Exception("Votação deve ter um período mínimo de 6 horas. Para alterar o horário de abertura deve ser alterado também o de encerramento [2].");

                                        alterarDataInicioVotacao = true;
                                    }
                                    else if (!string.IsNullOrEmpty(votacao.horaFim))
                                    {
                                        string[] arrayInicio = horaInicioSistema.Split(':');
                                        int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                        string[] arrayFim = votacao.horaFim.Split(':');
                                        int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                        int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                        if (periodoVotacao <= 5)
                                            throw new Exception("Votação deve ter um período mínimo de 6 horas. Para alterar o horário de encerramento deve ser alterado também o de abertura [2].");

                                        alterarDataFimVotacao = true;
                                    }
                                }
                            }
                        }
                        else if (dtVotacaoAtual.Contains("01/01/0001") || votacao.dtVotacao.Date == dtVotacaoSistema.Date) //Alterar Horário
                        { //Se informar apenas o horário para alteração, a aplicação usará como base a data da votação que consta no Banco de Dados
                            if (dtVotacaoSistema.Date == DateTime.Now.Date) //Alterar horário de votação para hoje: precisa validar horário
                            {
                                if (!string.IsNullOrEmpty(votacao.horaInicio) && !string.IsNullOrEmpty(votacao.horaFim)) //Altera hora abertura e encerramento
                                {
                                    string[] arrayInicio = votacao.horaInicio.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);
                                    if (horaVotacaoInicio <= DateTime.Now.Hour)
                                        throw new Exception("Deve ser informada uma hora maior que a atual [2].");

                                    string[] arrayFim = votacao.horaFim.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas [4].");

                                    alterarHoraInicioFim = true;
                                }
                                else
                                {
                                    throw new Exception("Para atualizar uma votação de hoje, deve ser informado novos horários de abertura e encerramento.");
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(votacao.horaInicio) && !string.IsNullOrEmpty(votacao.horaFim)) //Altera hora abertura e encerramento
                                {
                                    string[] arrayInicio = votacao.horaInicio.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                    string[] arrayFim = votacao.horaFim.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas [4].");

                                    alterarHoraInicioFim = true;

                                }
                                else if (!string.IsNullOrEmpty(votacao.horaInicio)) //Alterar hora abertura
                                {
                                    string[] arrayInicio = votacao.horaInicio.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                    string[] arrayFim = horaFimSistema.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas. Para alterar o horário de abertura deve ser alterado também o de encerramento [3].");

                                    alterarHoraInicio = true;
                                }
                                else if (!string.IsNullOrEmpty(votacao.horaFim)) //Alterar hora encerramento
                                {
                                    string[] arrayInicio = horaInicioSistema.Split(':');
                                    int horaVotacaoInicio = Int32.Parse(arrayInicio[0]);

                                    string[] arrayFim = votacao.horaFim.Split(':');
                                    int horaVotacaoFim = Int32.Parse(arrayFim[0]);
                                    int periodoVotacao = horaVotacaoFim - horaVotacaoInicio;
                                    if (periodoVotacao <= 5)
                                        throw new Exception("Votação deve ter um período mínimo de 6 horas. Para alterar o horário de encerramento deve ser alterado também o de abertura [3].");

                                    alterarHoraFim = true;
                                }
                            }
                        }
                        #endregion

                        #region Scripts Alterar Data
                        if (alterarDataInicioFimVotacao)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET dtVotacao = '" + votacao.dtVotacao.Date + "', horaInicio = '" + votacao.horaInicio + "', horaFim = '" + votacao.horaFim + "' WHERE idVotacao = " + idVotacao);
                        }
                        else if (alterarDataInicioVotacao)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET dtVotacao = '" + votacao.dtVotacao.Date + "', horaInicio = '" + votacao.horaInicio + "' WHERE idVotacao = " + idVotacao);
                        }
                        else if (alterarDataFimVotacao)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET dtVotacao = '" + votacao.dtVotacao.Date + "', horaFim = '" + votacao.horaFim + "' WHERE idVotacao = " + idVotacao);
                        }
                        else if (alterarDataVotacao)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET dtVotacao = '" + votacao.dtVotacao.Date + "' WHERE idVotacao = " + idVotacao);
                        }
                        #endregion

                        #region Scripts Alterar Hora
                        if (alterarHoraInicioFim)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET horaInicio = '" + votacao.horaInicio + "', horaFim = '" + votacao.horaFim + "' WHERE idVotacao = " + idVotacao);
                        }
                        else if (alterarHoraInicio)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET horaInicio = '" + votacao.horaInicio + "' WHERE idVotacao = " + idVotacao);
                        }
                        else if (alterarHoraFim)
                        {
                            scriptUpdate.Add("UPDATE tbVotacao SET horaFim = '" + votacao.horaFim + "' WHERE idVotacao = " + idVotacao);
                        }
                        #endregion

                        if (scriptUpdate.Count > 0)
                        {
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = conn;

                            foreach (string script in scriptUpdate)
                            {
                                cmd.CommandText = script;

                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    if (conn != null)
                                        conn.Close();

                                    throw new Exception("Falha ao Alterar Votação: " + ex.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Não pode ser alterada uma Votação que está aberta.");
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
                if (conn != null)
                    conn.Close();

                dsErro = "Falha no processo de Alteração: " + ex.Message;
            }

            conn.Close();
            return dsErro;
        }

        public RetornoVotacao GetVotacao()
        {
            RetornoVotacao retorno = new RetornoVotacao();
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
                    dtDados = VerificarVotacao(DateTime.Now.Year, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    retorno.idVotacao = Int32.Parse(dtDados.Rows[0]["idVotacao"].ToString());
                    retorno.dsTema = dtDados.Rows[0]["tema"].ToString();
                    retorno.dsAno = Int32.Parse(dtDados.Rows[0]["ano"].ToString());
                    retorno.dtVotacao = DateTime.Parse(dtDados.Rows[0]["dtVotacao"].ToString());
                    retorno.dsHoraInicio = dtDados.Rows[0]["horaInicio"].ToString();
                    retorno.dsHoraFim = dtDados.Rows[0]["horaFim"].ToString();
                    retorno.flAberto = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de uma votação encontrada. Contate a Equipe Técnica");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                throw new Exception("Falha ao retornar dados da Votação: " + ex.Message);
            }

            conn.Close();
            return retorno;
        }

        public string AbrirVotacao()
        {
            SqlConnection conn = new SqlConnection(conexao.connectionString);
            DataTable dtDados = new DataTable();
            string dsErro = string.Empty;

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
                    dtDados = VerificarVotacao(DateTime.Now.Year, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    DateTime dtVotacao = DateTime.Parse(dtDados.Rows[0]["dtVotacao"].ToString());
                    bool votacaoAberta = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());
                    bool votacaoEncerrada = Boolean.Parse(dtDados.Rows[0]["flVotacaoEncerrada"].ToString());

                    if (DateTime.Now.Date < dtVotacao)
                        throw new Exception("Votação só pode ser aberta na data informada.");

                    if (votacaoAberta)
                        throw new Exception("Votação já está aberta.");

                    if (!votacaoAberta && votacaoEncerrada)
                        throw new Exception("Votação encerrada não pode ser aberta.");

                    #region Abrir Votação
                    string scriptUpdate = "UPDATE tbVotacao SET flAberto = 1 WHERE ano = @ano";

                    SqlCommand cmd = new SqlCommand(scriptUpdate, conn);
                    cmd.Parameters.AddWithValue("@ano", DateTime.Now.Year);

                    cmd.ExecuteNonQuery();
                    #endregion
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de uma votação encontrada. Contate a Equipe Técnica");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                dsErro = "Falha ao abrir Votação: " + ex.Message;
            }

            conn.Close();
            return dsErro;
        }

        public string EncerrarVotacao()
        {
            SqlConnection conn = new SqlConnection(conexao.connectionString);
            DataTable dtDados = new DataTable();
            string dsErro = string.Empty;

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
                    dtDados = VerificarVotacao(DateTime.Now.Year, conn);
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw new Exception("Falha ao realizar consulta no Banco de Dados: " + ex.Message);
                }

                if (dtDados.Rows.Count == 1)
                {
                    bool votacaoAberta = Boolean.Parse(dtDados.Rows[0]["flAberto"].ToString());
                    bool votacaoEncerrada = Boolean.Parse(dtDados.Rows[0]["flVotacaoEncerrada"].ToString());

                    if (!votacaoAberta)
                        throw new Exception("Votação não está aberta.");

                    #region Encerrar Votação
                    string scriptUpdate = "UPDATE tbVotacao SET flAberto = 0, flVotacaoEncerrada = 1 WHERE ano = @ano";

                    SqlCommand cmd = new SqlCommand(scriptUpdate, conn);
                    cmd.Parameters.AddWithValue("@ano", DateTime.Now.Year);

                    cmd.ExecuteNonQuery();
                    #endregion
                }
                else if (dtDados.Rows.Count > 1)
                {
                    throw new Exception("Mais de uma votação encontrada. Contate a Equipe Técnica");
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                dsErro = "Falha ao encerrar Votação: " + ex.Message;
            }

            conn.Close();
            return dsErro;
        }

        #region Métodos

        #region Verificar Votacao
        public DataTable VerificarVotacao(int ano, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            string script = "SELECT idVotacao, tema, edicao, ano, dtVotacao, horaInicio, horaFim, flAberto, flVotacaoEncerrada FROM tbVotacao (NOLOCK) WHERE ano = @ano";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("@ano", ano);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            return dtDados;
        }
        #endregion

        #region Cadastrar Votação
        public static string InserirVotacao(Votacao votacao, SqlConnection conn)
        {
            string erro = string.Empty;

            string scriptInsert = "INSERT INTO tbVotacao (tema, edicao, ano, dtVotacao, horaInicio, horaFim, idUserInsert) VALUES (@tema, @edicao, @ano, @dtVotacao, @horaInicio, @horaFim, @idUserInsert)";

            //Adicionar Parâmetros
            SqlCommand cmd = new SqlCommand(scriptInsert, conn);
            cmd.Parameters.AddWithValue("@tema", votacao.getTema());
            cmd.Parameters.AddWithValue("@edicao", votacao.getEdicao());
            cmd.Parameters.AddWithValue("@ano", votacao.getAno());
            cmd.Parameters.AddWithValue("@dtVotacao", votacao.getDtVotacao());
            cmd.Parameters.AddWithValue("@horaInicio", votacao.getHoraInicio());
            cmd.Parameters.AddWithValue("@horaFim", votacao.getHoraFim());
            cmd.Parameters.AddWithValue("@idUserInsert", votacao.getIdUserInsert());

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                erro = "Falha ao Inserir Votação: " + ex.Message;
            }

            return erro;
        }
        #endregion

        #region Consultar Votação
        public DataTable ConsultarVotacao(int idVotacao, SqlConnection conn)
        {
            DataTable dtDados = new DataTable();

            string script = "SELECT idVotacao, edicao, ano, dtVotacao, horaInicio, horaFim, flAberto FROM tbVotacao (NOLOCK) WHERE idVotacao = @idVotacao";

            //Adicionar parâmetros
            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.Parameters.AddWithValue("idVotacao", idVotacao);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtDados);

            return dtDados;
        }
        #endregion

        #endregion
    }
}