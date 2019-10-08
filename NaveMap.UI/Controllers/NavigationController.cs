using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NaveMap.UI.Models;

namespace NaveMap.UI.Controllers
{
    public class NavigationController : Controller
    {
        public IActionResult Navigate()
        {
            return View();
        }

        [HttpPost]   
        public IActionResult Navigate(NavigationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                if (!InputValidation(model, out string errorMessage))
                {
                    switch (errorMessage.Split(';')[0])
                    {
                        case "01":
                            ViewData["ValidationError01"] = errorMessage.Split(';')[1];
                            break;
                        case "02":
                            ViewData["ValidationError02"] = errorMessage.Split(';')[1];
                            break;
                        case "03":
                            ViewData["ValidationError03"] = errorMessage.Split(';')[1];
                            break;
                    }                   

                    return View(model);
                }

                foreach(var nave in model.ListNaves)
                {
                    var posicaoInicial = nave.CoordinateRef.ToUpper().Replace(" ", "").Split(',');
                    var navegacao = nave.CoordinateNav.ToUpper().Replace(" ", "").ToArray();

                    nave.CoordinateFinal = CommandResult(posicaoInicial, navegacao);
                }

                return View(model);
            }
        }

        public bool InputValidation(NavigationViewModel model, out string errorMessage)
        {
            // MAPA
            var mapa = model.CoordinateMap.Replace(" ", "").Split(',');

            if (model.CoordinateMap.Any(c => char.IsLetter(c)))
            {
                errorMessage = "01-Por favor, inclua somente números separados por vírgula.";
                return false;
            }

            if (mapa.Length != 2)
            {
                errorMessage = "01-Por favor, inclua uma vírgula para separar as coordenadas (x,y).";
                return false;
            }

            if (mapa.Where(c=> string.IsNullOrWhiteSpace(c)).ToList().Count() > 0)
            {
                errorMessage = "01-Por favor, inclua as coordenadas em formato correto (x,y).";
                return false;
            }

            int i = 0;

            foreach(var nave in model.ListNaves)
            {
                // POSICAO INICIAL
                var posicaoInicial = nave.CoordinateRef.ToUpper().Replace(" ", "").Split(',');

                if (posicaoInicial.Length != 3)
                {
                    errorMessage = $"02;{i}-Por favor, inclua duas vírgulas para separar as coordenadas e a direção (x,y,D).";
                    return false;
                }

                if (posicaoInicial.Where(c => string.IsNullOrWhiteSpace(c)).ToList().Count() > 0)
                {
                    errorMessage = $"02;{i}-Por favor, inclua as coordenadas e a direção em formato correto (x,y,D).";
                    return false;
                }

                if (!(posicaoInicial[0].Any(c => char.IsNumber(c)) && posicaoInicial[1].Any(c => char.IsNumber(c)) && posicaoInicial[2].Any(c => char.IsLetter(c))))
                {
                    errorMessage = $"02;{i}-Por favor, inclua as coordenadas e a direção no formato correto (número,número,letra).";
                    return false;
                }

                if (posicaoInicial[2] != "N" && posicaoInicial[2] != "E" && posicaoInicial[2] != "S" && posicaoInicial[2] != "W")
                {
                    errorMessage = $"02;{i}-Por favor, inclua somente a direção indicada (N, S, E, W).";
                    return false;
                }

                if (Convert.ToInt32(mapa[0]) < Convert.ToInt32(posicaoInicial[0]) || Convert.ToInt32(mapa[1]) < Convert.ToInt32(posicaoInicial[1]))
                {
                    errorMessage = $"02;{i}-A localização da sonda está fora do mapa.";
                    return false;
                }

                // NAVEGACAO
                var navegacao = nave.CoordinateNav.ToUpper().Replace(" ", "").ToArray();

                if (nave.CoordinateNav.Any(c => char.IsNumber(c)))
                {
                    errorMessage = $"03;{i}-Por favor, inclua somente letras.";
                    return false;
                }

                if (navegacao.Distinct().Except("R").Except("L").Except("M").ToList().Count > 0)
                {
                    errorMessage = $"03;{i}-Por favor, inclua somente as letras indicadas (L, R, M).";
                    return false;
                }

                if (navegacao.Where(c => char.IsWhiteSpace(c)).ToList().Count() > 0)
                {
                    errorMessage = $"03;{i}-Por favor, inclua somente as letras indicadas (L, R, M).";
                    return false;
                }

                i++;
            }

            errorMessage = "";
            return true;
        }

        public string CommandResult(string[] posicaoInicial, char[] navegacao)
        {
            var xInicial = posicaoInicial[0];
            var yInicial = posicaoInicial[1];
            var dInicial = posicaoInicial[2];

            var xAux = Convert.ToInt32(xInicial);
            var yAux = Convert.ToInt32(yInicial);
            var dAux = dInicial;

            foreach (var nav in navegacao)
            {
                switch (nav)
                {
                    case 'L':
                        var dir = TurnLeft(dAux);
                        dAux = dir;
                        break;
                    case 'R':
                        dir = TurnRight(dAux);
                        dAux = dir;
                        break;
                    case 'M':
                        var destinoNav = Move(xAux, yAux, dAux);
                        xAux = Convert.ToInt32(destinoNav.Split(',')[0]);
                        yAux = Convert.ToInt32(destinoNav.Split(',')[1]);
                        break;
                }
            }

            return xAux.ToString() + "," + yAux.ToString() + "," + dAux;
        }

        public string TurnLeft(string dInicial)
        {
            string dFinal = "";

            switch (dInicial)
            {
                case "N":
                    dFinal = "W";
                    break;
                case "W":
                    dFinal = "S";
                    break;
                case "S":
                    dFinal = "E";
                    break;
                case "E":
                    dFinal = "N";
                    break;
            }

            return dFinal;
        }

        public string TurnRight(string dInicial)
        {
            string dFinal= "";

            switch (dInicial)
            {
                case "N":
                    dFinal = "E";
                    break;
                case "E":
                    dFinal = "S";
                    break;
                case "S":
                    dFinal = "W";
                    break;
                case "W":
                    dFinal = "N";
                    break;
            }

            return dFinal;
        }

        public string Move(int x, int y, string d)
        {
            switch (d)
            {
                case "N":
                    y = y + 1;
                    break;
                case "E":
                    x = x + 1;
                    break;
                case "S":
                    y = y - 1;
                    break;
                case "W":
                    x = x - 1;
                    break;
            }

            var result = x.ToString() + "," + y.ToString() + "," + d;

            return result;
        }
    }
}