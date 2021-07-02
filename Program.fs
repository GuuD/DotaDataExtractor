
(*
    Print all matches of a team with specific hero in play, we should take into account only matches played on recent patch.
        
        1. Show ordered list of the Dota pro teams
        2. Read input from the user, selecting one of the teams
        3. Read input from the user, selecting hero
        4. Iff both inputs are valid, start web request sequence
            1. Request all matches of selected team
            2. Enrich each of them with data from get match request
            3. Filter matches by checking whether selected hero was played
        5. Display list of matches (date, and teams involved)
    
*)


    
open Thoth.Json.Net

module OpenDota =
    open Oryx
    open FSharp.Control.Tasks
    open System.Net.Http
    open Oryx.ThothJsonNet.ResponseReader
    let teamId = 15
    
    
    
    type MatchOutcome =
        | Victory
        | Defeat
        
    type GameStats = {
        DurationInMinutes: int
        OpposingTeamName: string
        Outcome: MatchOutcome
    }
    
    let gameStatsDecoder: Decoder<GameStats> =
        let matchDataToStats isRadiant didRadiantWin duration opponents =
            {
                DurationInMinutes = duration / 60
                OpposingTeamName = opponents
                Outcome = if isRadiant = didRadiantWin then Victory else Defeat
            } 
        Decode.map4
            matchDataToStats
            (Decode.field "radiant_win" Decode.bool)
            (Decode.field "radiant" Decode.bool)
            (Decode.field "duration" Decode.int)
            (Decode.field "opposing_team_name" Decode.string)
    let gameHistoryDecoder = Decode.list gameStatsDecoder
    
    let requestAllMatchesForTeam (teamId: int) =
        let url = $"https://api.opendota.com/api/teams/{teamId}/matches"
        GET
        >=> withUrl url
        >=> fetch
        >=> json gameHistoryDecoder
        
        
    let run () = task {
        use client = new HttpClient ()
        let ctx =
            HttpContext.defaultContext
            |> HttpContext.withHttpClient client
        let! result = runAsync ctx (requestAllMatchesForTeam teamId)
        printfn $"Result: %A{result}"   
    }



open OpenDota

open System
[<EntryPoint>]
let main argv =
    run().GetAwaiter().GetResult()
    Console.ReadKey() |> ignore
    0