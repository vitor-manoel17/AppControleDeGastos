using Microsoft.AspNetCore.Mvc;
using AppControleDeGastos.Data;
using AppControleDeGastos.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using AppControleDeGastos.Models.CategoriaModel;

namespace AppControleDeGastos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categoria
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            var categorias = await _context.Categorias
                                           .Where(c => c.DataDelecao == null) 
                                           .OrderBy(c => c.Nome)              // Ordenar categorias pelo nome
                                           .Select(c => new
                                           {
                                               c.CategoriaId,
                                               c.Nome,
                                               c.DataCriacao
                                           })  
                                           .ToListAsync();

            if (!categorias.Any())
            {
                return NotFound(new { Success = false, Message = "Nenhuma categoria encontrada." });
            }

            return Ok(new { Success = true, Message = "Categorias encontradas com sucesso.", Categorias = categorias });
        }

        // GET: api/Categoria/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            var categoria = await _context.Categorias
                                          .Where(c => c.CategoriaId == id && c.DataDelecao == null)
                                          .Select(c => new
                                          {
                                              c.CategoriaId,
                                              c.Nome,
                                              c.DataCriacao
                                          })  
                                          .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(new { Success = false, Message = "Categoria não encontrada." });
            }

            return Ok(new
            {
                Success = true,
                Message = "Categoria encontrada com sucesso.",
                Categoria = categoria
            });
        }

        // POST: api/Categoria
        [HttpPost]
        public async Task<ActionResult> AddCategoria(Categoria categoria)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            // Validação: Verificar se já existe uma categoria com o mesmo nome
            var categoriaExistente = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome == categoria.Nome);

            if (categoriaExistente != null)
            {
                return BadRequest(new { Success = false, Message = "Já existe uma categoria com este nome." });
            }

            categoria.DataCriacao = DateTime.UtcNow;
            categoria.DataAtualizacao = DateTime.UtcNow;

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var resposta = new
            {
                Success = true,
                Message = "Categoria criada com sucesso.",
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome
            };

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.CategoriaId }, resposta);
        }


        // PUT: api/Categoria/5
        [HttpPut]
        public async Task<IActionResult> UpdateCategoria(Categoria categoria)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            if (categoria.CategoriaId == 0)
            {
                return BadRequest(new { Success = false, Message = "ID da categoria não fornecido." });
            }

            var categoriaExistente = await _context.Categorias
                                                   .Where(c => c.CategoriaId == categoria.CategoriaId && c.DataDelecao == null)
                                                   .FirstOrDefaultAsync();

            if (categoriaExistente == null)
            {
                return NotFound(new { Success = false, Message = "Categoria não encontrada." });
            }

            categoriaExistente.Nome = categoria.Nome;
            categoriaExistente.DataAtualizacao = DateTime.UtcNow; 

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var categoriaAindaExiste = await _context.Categorias
                                                         .AnyAsync(e => e.CategoriaId == categoria.CategoriaId && e.DataDelecao == null);

                if (!categoriaAindaExiste)
                {
                    return NotFound(new { Success = false, Message = "Categoria não encontrada após tentativa de atualização." });
                }
                else
                {
                    throw; 
                }
            }

            return Ok(new { Success = true, Message = "Categoria atualizada com sucesso.", CategoriaId = categoria.CategoriaId, Nome = categoria.Nome });
        }


        // DELETE: api/Categoria/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            var categoria = await _context.Categorias
                                          .Where(c => c.CategoriaId == id && c.DataDelecao == null)
                                          .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(new { Success = false, Message = "Categoria não encontrada ou já deletada." });
            }

            categoria.DataDelecao = DateTime.UtcNow;
            categoria.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Categoria deletada com sucesso." });
        }

    }
}
