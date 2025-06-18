using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using CaotinhoAuMiau.Models;
using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class PetViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string Raca { get; set; } = string.Empty;
        public int Anos { get; set; }
        public int Meses { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Porte { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = "Disponível";
        public string? NomeArquivoImagem { get; set; }

        public List<Pet> Pets { get; set; } = new List<Pet>();
        
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItens { get; set; } = 0;
        public bool TemPaginaAnterior => PaginaAtual > 1;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;

        public string FiltroEspecie { get; set; } = string.Empty;
        public string FiltroSexo { get; set; } = string.Empty;
        public string FiltroPorte { get; set; } = string.Empty;
        public string FiltroIdade { get; set; } = string.Empty;
        public string TermoBusca { get; set; } = string.Empty;
        public string FiltroRaca { get; set; } = string.Empty;
        public string FiltroOrdem { get; set; } = string.Empty;
        public int ItensPorPaginaSelecionado { get; set; } = 12;
        public string FiltroNome { get; set; } = string.Empty;

        public SelectList Especies { get; set; } = new SelectList(new List<SelectListItem>());
        public SelectList Sexos { get; set; } = new SelectList(new List<SelectListItem>());
        public SelectList Portes { get; set; } = new SelectList(new List<SelectListItem>());
        public SelectList FaixasEtarias { get; set; } = new SelectList(new List<SelectListItem>());


        public static PetViewModel CriarDaEntidade(Pet pet)
        {
            return new PetViewModel
            {
                Id = pet.Id,
                Nome = pet.Nome ?? string.Empty,
                Especie = pet.Especie ?? string.Empty,
                Raca = pet.Raca ?? string.Empty,
                Anos = pet.Anos,
                Meses = pet.Meses,
                Sexo = pet.Sexo ?? string.Empty,
                Porte = pet.Porte ?? string.Empty,
                Descricao = pet.Descricao ?? string.Empty,
                Status = pet.Status ?? "Disponível",
                NomeArquivoImagem = pet.NomeArquivoImagem
            };
        }

        public static PetViewModel CriarParaListagem()
        {
            return new PetViewModel
            {
                Pets = new List<Pet>(),
                PaginaAtual = 1,
                TotalPaginas = 1,
                TotalItens = 0,
                Especies = new SelectList(new List<SelectListItem>()),
                Sexos = new SelectList(new List<SelectListItem>()),
                Portes = new SelectList(new List<SelectListItem>()),
                FaixasEtarias = new SelectList(new List<SelectListItem>())
            };
        }
    }
} 