﻿using Lexical;
using Microsoft.AspNetCore.Mvc;
using Syntax;

namespace CSharpParserGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        enum ELangTokens
        {
            Ignore,

            LParenthesis,
            RParenthesis,

            And,
            Or,

            Eq,
            Gt,
            Lt,

            Number,
            String,
            Boolean,
            Var
        }
        public enum ELangRules
        {
            S,
            E,
            T,

            //
            Int,
            Para,
            Parc,
            Plus,
            Mul
        }

        [HttpGet]
        [Route("/lexer")]
        public IActionResult ProcessExpression([FromQuery] string expression)
        {
            var tokenDefinitions = new TokenDefinitions<ELangTokens>
            {
                [ELangTokens.LParenthesis] = "\\(",
                [ELangTokens.RParenthesis] = "\\)",
                [ELangTokens.And] = "(?i)and",
                [ELangTokens.Or] = "(?i)or",
                [ELangTokens.Eq] = "(?i)eq",
                [ELangTokens.Gt] = "(?i)gt",
                [ELangTokens.Lt] = "(?i)lt",
                [ELangTokens.Number] = "[-+]?\\d*(\\.\\d+)?",
                [ELangTokens.String] = "(\".*\"|'.*')",
                [ELangTokens.Boolean] = "(true|false)",
                [ELangTokens.Var] = "[_a-zA-Z][_a-zA-Z\\d]*",
                [ELangTokens.Ignore] = "[ \\n]+"
            };

            var lexer = new Lexer<ELangTokens>(tokenDefinitions);
            var lexerResult = lexer.ProcessExpression(expression, ELangTokens.Ignore);

            if (!lexerResult.Success) return BadRequest(lexerResult);
            return Ok(lexerResult);
        }



        [HttpGet]
        [Route("/parser")]
        public IActionResult Parser()
        {

            var rules = new GrammarRules<ELangRules>
            {
                [ELangRules.S] = new ProductionRules
                {
                    new SymbolNode[] { ELangRules.E },
                },
                [ELangRules.E] = new ProductionRules
                {
                   new SymbolNode[] { ELangRules.T, ELangRules.Int, ELangRules.E, new Op(_ => {})},
                    new SymbolNode[] { ELangRules.T },
                },
                [ELangRules.T] = new ProductionRules
                {
                    new SymbolNode[] { ELangRules.Int, new Op(_ => {}), ELangRules.Mul, ELangRules.T},
                    new SymbolNode[] { ELangRules.Int },
                    new SymbolNode[] { ELangRules.Para, ELangRules.E, ELangRules.Parc },
                }
            };

            var parser = new ParserGenerator<ELangRules>(rules);
            parser.CompileParser();

            return Ok();
        }
    }
}
