﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
    // api/peliculas/3/reviews
    [Route("api/peliculas/{peliculaId:int}/reviews")]
    public class ReviewController: CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReviewController(ApplicationDbContext context,
            IMapper mapper)
            :base(context, mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReviewDTO>>> Get( int peliculaId,
            [FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _context.Reviews.Include(r => r.Usuario).AsQueryable();
            queryable = queryable.Where(p => p.PeliculaId == peliculaId);
            return await Get<Review, ReviewDTO>(paginacionDTO, queryable);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int peliculaId, [FromBody] ReviewCreacionDTO reviewCreacionDTO)
        {
            var existePelicula = await _context.Peliculas.AnyAsync(p => p.Id == peliculaId);
            if (!existePelicula)
            {
                return NotFound();
            }

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;

            var reviewExiste = 
                await _context.Reviews.AnyAsync(r => r.PeliculaId == peliculaId && r.UsuarioId == usuarioId);
            if (reviewExiste)
            {
                return BadRequest("El usuario ya ha escriot un review de esta película..!");
            }

            var review = _mapper.Map<Review>(reviewCreacionDTO);
            review.PeliculaId = peliculaId;
            review.UsuarioId = usuarioId;

            _context.Add(review);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
