using Microsoft.AspNetCore.Mvc;

namespace BancoSENAIAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DocumentoController : Controller
    {
        private readonly string _caminhoRaiz = Path.Combine(
            Directory.GetCurrentDirectory(),
            "ClienteArquivos"
            );

        private static List<Models.DocumentoMetadados> _documentosMetaDados = new List<Models.DocumentoMetadados>();

        private static int _nexId = 1;

        [HttpPost("upload/{codigoCliente}")]
        public async Task<IActionResult> AnexarArquivo(int codigoCliente, IFormFile arquivo)
        {
            if (arquivo.Length > 2 * 1024 * 1024)
            {
                return BadRequest("O arquivo não pode ter mais de 2 MB.");
            }
            string pastaCliente = Path.Combine(_caminhoRaiz, codigoCliente.ToString());
            if (!Directory.Exists(pastaCliente))
            {
                Directory.CreateDirectory(pastaCliente);
            }
            string extensao = Path.GetExtension(arquivo.FileName);
            if (extensao != ".pdf" && extensao != ".jpg" && extensao != ".png")
            {
                return BadRequest("Extensão de arquivo não permitida. Use apenas .pdf, .jpg ou .png.");
            }

            string nameOriginal = Path.GetFileNameWithoutExtension(arquivo.FileName);
            string novoNome = $"{codigoCliente}_{nameOriginal}_{Guid.NewGuid()}{extensao}";
            string caminhoFinal = Path.Combine(_caminhoRaiz, novoNome);

            using (var stream = new FileStream(caminhoFinal, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }
            var documentoMetaDados = new Models.DocumentoMetadados
            {
                id = _nexId++,
                name = nameOriginal,
                Extensao = extensao,
                Caminho = caminhoFinal,
                CodigoCliente = codigoCliente,
            };
            _documentosMetaDados.Add(documentoMetaDados);
            return Ok(new { mensagem = "Documento anexado com sucesso", arquivoSalvo = novoNome });

        }
        [HttpGet("listar/{codigoCliente}")]
        public IActionResult ListarDocumentos(int codigoCliente)
        {
            var documentos = _documentosMetaDados
                .Where(d => d.CodigoCliente == codigoCliente)
                .ToList();

            if (!documentos.Any())
            {
                return NotFound("Nenhum documento encontrado para este cliente.");
            }

            return Ok(documentos);
        }
        [HttpGet("download/{id}")]
        public IActionResult Download(int id)
        {
            var documento = _documentosMetaDados
                .FirstOrDefault(d => d.id == id);

            if (documento == null)
            {
                return NotFound("Documento não encontrado.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(documento.Caminho);

            string nomeArquivo = documento.name + documento.Extensao;

            return File(
                fileBytes,
                "application/octet-stream",
                nomeArquivo
            );
        }
        [HttpDelete("excluir/{id}")]
        public IActionResult Excluir(int id)
        {
            var documento = _documentosMetaDados
                .FirstOrDefault(d => d.id == id);

            if (documento == null)
            {
                return NotFound("Documento não encontrado.");
            }

            System.IO.File.Delete(documento.Caminho);

            _documentosMetaDados.Remove(documento);

            return Ok("Documento excluído com sucesso.");
        }
    }
}