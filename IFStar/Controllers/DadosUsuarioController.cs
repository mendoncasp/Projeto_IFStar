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
    public class DadosUsuarioController : ApiController
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

        public HttpResponseMessage Post(Param param)
        {
            string dsErro = string.Empty;

            try
            {
                Param parametro = new Param();
                parametro = param;
                parametro.ValidarParam(parametro);

                int cdPrivilegio = string.IsNullOrEmpty(param.cdPrivilegio) ? 1 : Convert.ToInt32(param.cdPrivilegio);
                Usuario usuario = new Usuario(parametro.dsNome, parametro.dsCpf, parametro.dsEmail, parametro.dsSenha, cdPrivilegio);

                GatewayDadosUsuario dados = new GatewayDadosUsuario();
                dsErro = dados.AddUsuario(usuario);

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

        public HttpResponseMessage Get()
        {
            try
            {
                Dictionary<string, string> queryString = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
                GatewayDadosUsuario dados = new GatewayDadosUsuario();
                Retorno retorno = new Retorno();
                string dsErro = string.Empty;

                string outValue = null;
                queryString.TryGetValue("" + (int)Campos.TipoServico, out outValue);
                int tipoServico = Int32.Parse(queryString["" + (int)Campos.TipoServico]);

                if (tipoServico == 1) //Login Usuário
                {
                    retorno = dados.GetUsuario(queryString);
                    return Request.CreateResponse<Retorno>(HttpStatusCode.OK, retorno);
                }
                else if (tipoServico == 2) //Recuperação de Senha
                {
                    dsErro = dados.RecuperarSenha(queryString);

                    if (string.IsNullOrEmpty(dsErro))
                        return Request.CreateResponse(HttpStatusCode.Created, dsErro);
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

        public HttpResponseMessage Put(ParamAlteracao param)
        {
            string dsErro = string.Empty;

            try
            {
                GatewayDadosUsuario dados = new GatewayDadosUsuario();
                dsErro = dados.AlterUsuario(param);

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
    }
}
