using BancoSENAIAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BancoSENAIAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AgenciaController : ControllerBase
    {
        private static List<Agencia> _agencias = new List<Agencia>
        {
            new Agencia { NumeroAgencia = 1001, Cidade = "Aracaju", SiglaEstado = "SE" },
            new Agencia { NumeroAgencia = 2002, Cidade = "São Paulo", SiglaEstado = "SP" },
            new Agencia { NumeroAgencia = 3003, Cidade = "Salvador", SiglaEstado = "BA" }
        };

        private readonly string _caminhoRaiz = Path.Combine(Directory.GetCurrentDirectory(), "ClienteArquivos");
        private static List<Models.DocumentoMetadado> _documentosMetadados = new List<Models.DocumentoMetadado>();
        private static int _next = 1;

        [HttpGet]
        public IActionResult ListarTodas()
        {
            return Ok(_agencias);
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] Agencia novaAgencia)
        {
            if (_agencias.Any(a => a.NumeroAgencia == novaAgencia.NumeroAgencia))
                return BadRequest(new { message = "Este número de agência já existe." });

            _agencias.Add(novaAgencia);
            return Created("", novaAgencia);
        }

        [HttpGet("{codigo}")]
        public IActionResult ConsultarPorCodigo(int codigo)
        {
            var agencia = _agencias.FirstOrDefault(a => a.NumeroAgencia == codigo);

            if (agencia == null)
                return NotFound(new { message = "Agência não encontrada." });

            return Ok(agencia);
        }

        [HttpPut("{codigo}")]
        public IActionResult Alterar(int codigo, [FromBody] Agencia agenciaAtualizada)
        {
            var agenciaExistente = _agencias.FirstOrDefault(a => a.NumeroAgencia == codigo);

            if (agenciaExistente == null) return NotFound();

            agenciaExistente.Cidade = agenciaAtualizada.Cidade;
            agenciaExistente.SiglaEstado = agenciaAtualizada.SiglaEstado;

            return NoContent();
        }

        [HttpDelete("{codigo}")]
        public IActionResult Excluir(int codigo)
        {
            var agencia = _agencias.FirstOrDefault(a => a.NumeroAgencia == codigo);

            if (agencia == null) return NotFound();

            _agencias.Remove(agencia);
            return Ok(new { message = "Agência excluída com sucesso." });
        }

        // --- MÉTODOS DE UPLOAD E VALIDAÇÃO DE ARQUIVOS (R06F e R06G) ---

        [HttpPost("upload/{codigoCliente}")]
        public async Task<IActionResult> AnexarArquivo(int codigoCliente, IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest("Nenhum arquivo foi enviado.");
            }

            // R06F: Limite de Tamanho de Arquivo (máximo 2 MB)
            if (arquivo.Length > 2 * 1024 * 1024)
            {
                return BadRequest("O arquivo não pode ter mais de 2 MB.");
            }

            // R06G: Validação de Extensões Permitidas (.pdf, .jpg, .png)
            string[] extensoesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };
            string extensaoArquivo = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensaoArquivo))
            {
                return BadRequest("Erro (R06G): Extensão de arquivo não permitida. Envie apenas arquivos .pdf, .jpg ou .png.");
            }

            // Processamento e salvamento do arquivo
            string pastaCliente = Path.Combine(_caminhoRaiz, codigoCliente.ToString());

            if (!Directory.Exists(pastaCliente))
            {
                Directory.CreateDirectory(pastaCliente);
            }

            string extensao = Path.GetExtension(arquivo.FileName);
            string nomeOriginal = Path.GetFileNameWithoutExtension(arquivo.FileName);
            string novoNome = $"{codigoCliente}_{nomeOriginal}_{Guid.NewGuid()}{extensao}";
            string caminhoFinal = Path.Combine(pastaCliente, novoNome);

            using (var stream = new FileStream(caminhoFinal, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            var documentoMetadados = new Models.DocumentoMetadado
            {
                ID = _next++,
                Name = nomeOriginal,
                Extensão = extensao,
                Caminho = caminhoFinal,
                CodigoCliente = codigoCliente
            };
            _documentosMetadados.Add(documentoMetadados);

            return Ok(new { mensagem = "Documento anexado com sucesso", arquivoSalvo = novoNome });
        }

        [HttpGet("listar-documentos/{codigoCliente}")]
        public IActionResult ListarDocumentos(int codigoCliente)
        {
            var documentos = _documentosMetadados
                .Where(d => d.CodigoCliente == codigoCliente)
                .ToList();

            if (!documentos.Any())
            {
                return NotFound("Nenhum documento encontrado para este cliente.");
            }

            return Ok(documentos);
        }
    }
}