using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class HelpCommands : ApplicationCommandModule
    {
        [SlashCommand("help", "Show all FrostBot commands.")]
        public Task HelpAsync(InteractionContext context) =>
            SlashCommandResponder.RespondAsync(context, BuildEnglishHelp());

        [SlashCommand("ajuda", "Mostra todos os comandos do FrostBot.")]
        public Task AjudaAsync(InteractionContext context) =>
            SlashCommandResponder.RespondAsync(context, BuildPortugueseHelp());

        private static string BuildPortugueseHelp() =>
            "**Comandos do FrostBot**\n\n" +
            "**Geral**\n" +
            "• `/register` — Registra você no banco (necessário para jogar).\n" +
            "• `/carteira` — Mostra suas FrostCoins.\n" +
            "• `/criaturas nome` — Mostra os dados de uma criatura.\n" +
            "• `/feiticos nome` — Mostra os dados de um feitiço.\n" +
            "• `/armadura nome` — Mostra os dados de uma armadura.\n" +
            "• `/armas nome` — Mostra os dados de uma arma.\n" +
            "• `/acessorios nome` — Mostra os dados de um acessório.\n" +
            "• `/consumiveis nome` — Mostra os dados de um consumível.\n" +
            "• `/feitos nome` — Mostra os dados de um feito.\n" +
            "• `/ajuda` — Mostra esta lista de comandos.\n" +
            "• Digite `hee` — FrostBot responde (às vezes com um meme).\n\n" +
            "**Trabalho**\n" +
            "• `/trabalhar` — Trabalha em um local aberto agora (1 stamina, cooldown de 2h).\n" +
            "• `/trabalhar-ajuda` — Regras do sistema de trabalho.\n" +
            "• `/trabalhar-opcao` — Tabela de locais de trabalho.\n" +
            "• Stamina recarrega para 5 à meia-noite (00:00).\n\n" +
            "**Dados / FrostCoins**\n" +
            "• `/dados aposta` — Joga dados apostando FrostCoins.\n" +
            "• `/dados-ajuda` — Regras e recompensas do jogo de dados.\n" +
            "• `/d roll` — Rola dados usando `2d20` ou `d20` (multiplicador padrão: 1).\n" +
            "• `/risk número` — Rola dois dados e calcula primeiro menos segundo.\n\n" +
            "**Diversão**\n" +
            "• `/pergunta pergunta` — Pergunte algo ao FrostBot.\n\n" +
            "**Facções**\n" +
            "• É um trabalho em progresso.\n\n" +
            "**Staff**\n" +
            "• `/users` — Lista usuários registrados (staff).";

        private static string BuildEnglishHelp() =>
            "**FrostBot Commands**\n\n" +
            "**General**\n" +
            "• `/register` — Register yourself in the database (required to play).\n" +
            "• `/wallet` — Shows your FrostCoins.\n" +
            "• `/creatures name` — Shows a creature's information.\n" +
            "• `/spells name` — Shows a spell's information.\n" +
            "• `/armour name` — Shows armour information.\n" +
            "• `/weapons name` — Shows a weapon's information.\n" +
            "• `/accessories name` — Shows accessory information.\n" +
            "• `/consumables name` — Shows consumable information.\n" +
            "• `/feats name` — Shows feat information.\n" +
            "• `/help` — Shows this command list.\n" +
            "• Type `hee` — FrostBot replies (sometimes with a meme).\n\n" +
            "**Work**\n" +
            "• `/work` — Work at a workplace open right now (1 stamina, 2h cooldown).\n" +
            "• `/work-help` — Work system rules.\n" +
            "• `/work-options` — Workplace table.\n" +
            "• Stamina refills to 5 at midnight (00:00).\n\n" +
            "**Dice / FrostCoins**\n" +
            "• `/dice bet` — Play dice by betting FrostCoins.\n" +
            "• `/dice-help` — Dice game rules and rewards.\n" +
            "• `/d roll` — Roll dice using `2d20` or `d20` (default multiplier: 1).\n" +
            "• `/risk number` — Roll two dice and calculate first minus second.\n\n" +
            "**Fun**\n" +
            "• `/askfrost question` — Ask FrostBot something.\n\n" +
            "**Factions**\n" +
            "• It's a work in progress.\n\n" +
            "**Staff**\n" +
            "• `/users` — List registered users (staff).";
    }
}
