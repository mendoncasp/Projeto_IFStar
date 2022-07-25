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
    public class DadosParticipanteController : ApiController
    {
        public HttpResponseMessage Post(ParamParticipante param)
        {
            string dsErro = string.Empty;

            try
            {
                ParamParticipante parametro = new ParamParticipante();
                parametro = param;
                parametro.ValidarParamParticipante(parametro);

                Participante participante = new Participante(parametro.dsNome, parametro.dsMusica, parametro.dsInstEnsino, parametro.dsTelefone);

                GatewayDadosParticipante dados = new GatewayDadosParticipante();
                dsErro = dados.AddParticipante(participante);

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
