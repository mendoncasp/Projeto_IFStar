using IFStar.Models;
using IFStar.Negocios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IFStar.Controllers
{
    public class DadosVotacaoController : ApiController
    {
        #region Campos
        public enum Campos
        {
            TipoServico = 103
        }
        #endregion

        public HttpResponseMessage Post(ParamVotacao param)
        {
            string dsErro = string.Empty;

            try
            {
                ParamVotacao parametro = new ParamVotacao();
                parametro = param;
                parametro.ValidarParamVotacao(parametro);

                Votacao votacao = new Votacao(parametro.dsTema, parametro.nrEdicao, parametro.nrAno, parametro.dtVotacao, parametro.horaInicio, parametro.horaFim, parametro.idUserInsert);

                GatewayDadosVotacao dados = new GatewayDadosVotacao();
                dsErro = dados.AddVotacao(votacao);

                if (string.IsNullOrEmpty(dsErro))
                    return Request.CreateResponse(HttpStatusCode.Created, dsErro);
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, dsErro);
            }
            catch (Exception ex)
            {
                HttpStatusCode codigoErro = HttpStatusCode.InternalServerError;
                if (ex.Message.Equals("401"))
                    codigoErro = HttpStatusCode.Unauthorized;
                dsErro = ex.Message;
                return Request.CreateResponse(codigoErro, dsErro);
            }
        }

        public HttpResponseMessage Put(ParamVotacao param)
        {
            string dsErro = string.Empty;

            try
            {
                GatewayDadosVotacao dados = new GatewayDadosVotacao();
                dsErro = dados.AlterVotacao(param);

                if (string.IsNullOrEmpty(dsErro))
                    return Request.CreateResponse(HttpStatusCode.OK, dsErro);
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, dsErro);
            }
            catch (Exception ex)
            {
                HttpStatusCode codigoErro = HttpStatusCode.InternalServerError;
                if (ex.Message.Equals("401"))
                    codigoErro = HttpStatusCode.Unauthorized;
                dsErro = ex.Message;
                return Request.CreateResponse(codigoErro, dsErro);
            }
        }

        public HttpResponseMessage Get()
        {
            try
            {
                Dictionary<string, string> queryString = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
                GatewayDadosVotacao dados = new GatewayDadosVotacao();
                RetornoVotacao retorno = new RetornoVotacao();
                string dsErro = string.Empty;

                string outValue = null;
                queryString.TryGetValue("" + (int)Campos.TipoServico, out outValue);
                int tipoServico = Int32.Parse(queryString["" + (int)Campos.TipoServico]);

                if (tipoServico == 1) //Consultar Votação
                {
                    retorno = dados.GetVotacao();
                    return Request.CreateResponse<RetornoVotacao>(HttpStatusCode.OK, retorno);
                }
                else if (tipoServico == 2) //Abrir votação
                {
                    dsErro = dados.AbrirVotacao();
                    if (string.IsNullOrEmpty(dsErro))
                        return Request.CreateResponse(HttpStatusCode.OK, dsErro);
                    else
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, dsErro);
                }
                else if (tipoServico == 3) //Encerrar Votação
                {
                    dsErro = dados.EncerrarVotacao();
                    if (string.IsNullOrEmpty(dsErro))
                        return Request.CreateResponse(HttpStatusCode.OK, dsErro);
                    else
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, dsErro);
                }
                else
                {
                    string mensagemErro = "Tipo de Serviço Inexistente";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, mensagemErro);
                }
            }
            catch (Exception ex)
            {
                HttpStatusCode codigoErro = HttpStatusCode.InternalServerError;
                if (ex.Message.Equals("401"))
                    codigoErro = HttpStatusCode.Unauthorized;
                return Request.CreateResponse(codigoErro, ex.Message);
            }
        }
    }
}
