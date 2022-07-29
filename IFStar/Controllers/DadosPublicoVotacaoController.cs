using IFStar.Models;
using IFStar.Negocios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IFStar.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DadosPublicoVotacaoController : ApiController
    {
        #region Campos
        public enum Campos
        {
            TipoServico = 103
        }
        #endregion

        public HttpResponseMessage Post(ParamVoto param)
        {
            string dsErro = string.Empty;

            try
            {
                ParamVoto parametro = new ParamVoto();
                parametro = param;
                parametro.ValidarParamVoto(parametro);

                PublicoVotacao publicoVotacao = new PublicoVotacao(parametro.idUsuario, parametro.idParticipante);

                GatewayDadosPublicoVotacao dados = new GatewayDadosPublicoVotacao();
                dsErro = dados.AddVoto(publicoVotacao);

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
    }
}
