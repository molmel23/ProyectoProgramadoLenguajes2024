﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoProgramadoLenguajes2024.Models
{
    public class NotaMedica
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Texto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int numeroColegiadoMedico { get; set; }

        [ForeignKey("NumeroColegiadoMedico")]
        public MedicoTratante MedicoTratante { get; set; }
    }
}
