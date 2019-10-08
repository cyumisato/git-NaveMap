using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NaveMap.UI.Models
{
    public class NavigationViewModel
    {
        [Required(ErrorMessage = "Campo de preenchimento obrigatório")]
        [Display(Name = "Coordenadas do mapa (x, y) - Exemplo: 5,5")]
        public string CoordinateMap { get; set; }

        public List<Nave> ListNaves { get; set; }
    }

    public class Nave
    {
        [Required(ErrorMessage = "Campo de preenchimento obrigatório")]
        [Display(Name = "Coordenadas (x, y) e Direção (N, S, E, W) do local atual da sonda - Exemplo: 1,2,S")]
        public string CoordinateRef { get; set; }

        [Required(ErrorMessage = "Campo de preenchimento obrigatório")]
        [Display(Name = "Comandos para navegar a sonda (rotacionar: L ou R, andar: M) - Exemplo: LMRM")]
        public string CoordinateNav { get; set; }

        public string CoordinateFinal { get; set; }
    }
}
