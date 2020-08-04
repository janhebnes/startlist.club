using System.Data.Entity.Migrations;
using FlightJournal.Web.Models.Training;

namespace FlightJournal.Web.Migrations.FlightContext
{
    internal static class TrainingFlights
    {
        public static void InitializeTrainingLessons(Models.FlightContext context)
        {
            // ren røv
            context.TrainingPrograms.RemoveRange(context.TrainingPrograms);


            var tpSPL_S = new Training2Program("SPL-S", "SPL, spilstart",
                    @"<b>Særligt for elever, der indtræder i træningsprogrammet fra en anden DTO/ATO:</b> <br>
                        Når elever, der er startet på uddannelsen til SPL - certifikat i en anden DTO / ATO, ønsker at fortsætte uddannelsen i DSvU’s DTO, skal den uddannelsesansvarlige på den flyveplads, hvor eleven ønsker at fortsætte uddannelsen, skabe sig overblik over, hvor langt eleven er kommet i sin uddannelse.<br>
                        Den uddannelsesansvarlige skal hvis muligt indhente oplysninger hos Assisterende Head of Training i den ATO / DTO, som eleven kommer fra.Inden uddannelsen kan fortsætte i DSvU’s DTO, skal den uddannelsesansvarlige flyve en eller flere flyvninger med eleven for at konstatere det aktuelle uddannelsesniveau.<br>
                        <br>
                        <b>Operationelle bestemmelser for at flyve første soloflyvning:</b> <br>
                        Ved første soloflyvning må vindhastigheden ikke overstige 20 kts, og sidevindskomponenten må ikke overstige 12 kts.Eleven skal forud for soloflyvningen have vist evnen til at kunne starte i den maksimale sidevindskomponent, og begrænsninger i skoleflyets håndbog skal under alle omstændigheder overholdes."
                )
                {Url = @"https://medlem.dsvu.dk/grp900/2129-uhb922-flyvelektioner-til-spl-certifikat-spilstart/file/menu-id-117" };


            var lessSPL_A0 =  new Models.Training.Training2Lesson(
                "A0",
                @"<h4>Introduktion til skoleflyet</h4> 
                Eleven skal instrueres i, hvordan skoleflyet er indrettet og hvilke elementer, der skal betjenes under flyvningen. <br>
                Lektionen bør gennemføres som det allerførste i elevens uddannelsen. Øvelsen i sig selv medfører ikke flyvning.<br>
                <h4>Faretilstande og nødudstyr</h4>
                Eleven skal instrueres i sin rolle, hvis der skulle opstå en faresituation under flyvning, og hvilke ting han skal gøre i en sådan situation. <br>
                Eleven skal endvidere instrueres i, at der under uddannelsen vil opstå situationer som led i træningen, og som ikke er hverken en faretilstand eller en nødsituation.<br>
                <h4>Sikkerhed og rapportering</h4> 
                Instruktøren skal vise eleven, hvordan han kan rapportere sikkerhedsmæssige oplevelser til DSvU’s Safety Management System, for at andre kan lære af denne oplevelse. 
                Han skal endvidere orientere om, hvordan en pilot forholder sig, hvis der sker hændelse eller havari, og hvordan en pilot kan lave en rapportering, hvis han gør sig skyldig i noget, som medfører overtrædelse af regler og bestemmelser<br>"
            )
            {
                Precondition = @"Eleven skal være fyldt 14 år for at kunne flyve solo efter afsluttet uddannelse som svæveflyvepilot"
            };
            var lessSPL_A1 = new Models.Training.Training2Lesson(
                "A1",
                @"<h4>Forberedelse til flyvning</h4> 
                Eleven skal orienteres om, hvad der skal være på plads, inden man kan flyve, og at enhver skolingsflyvning starter med en briefing om, hvad der skal ske under flyvningen.<br>
                <h4>Omstændigheder på jorden</h4>Eleven skal hjælpe med til at få fly ud af hangaren og hjælpe med til Dagligt Tilsyn.<br>
                <h4>Tilpasning i cockpit</h4>Eleven skal instrueres i, hvad han skal have med i flyet, for at tyngdepunktet ligger korrekt og for at sidde godt, så han kan nå styregrejerne og have et tilstrækkeligt udsyn.<br>
                <h4>Forberedelse inden start</h4>Eleven skal lære at lave cockpitcheck efter flyets checkliste. Han skal forberede sig mentalt på en afbrudt start samt lære, at wiren under spilstart skal udkobles, hvis min. hastighed eller max. hastighed i spilstart nås.<br>
                <h4>Tilvænningsflyvning</h4>Eleven skal orienteres om, hvordan flyveplads og terræn ser ud fra luften, og han skal lære at kigge ud inden ændring af flyveretning.<br>"
            );
            var lessSPL_A2 = new Models.Training.Training2Lesson(
                "A2",
                @"
<h4>Fartkontrol og brug af ror</h4>
Eleven skal lære at bruge visuelle referencer under flyvning – især flyets stilling ift. horisonten. Han skal have demonstreret virkningen af de enkelte ror og derved opleve, at kun kombination af rorene giver god flyvning. <br>
Han skal have demonstreret og senere selv prøve at bringe flyet i sin normale position ved ligeudflyvning under normal glid.<br>
Luftbremser og evt. flaps demonstreres, så eleven kan opleve virkningen
                "
            );
            var lessSPL_A3 = new Models.Training.Training2Lesson(
                "A3",
                @"
<h4>Ligeudflyvning</h4>
Eleven skal prøve at flyve med forskellige hastigheder for herefter at vende tilbage til normal flyvestilling ved brug af visuel reference til horisonten. <br>
Samtidig skal han overvåge instrumenter – især fartmåler og variometer. Han erfarer, at fartmåleren reagerer bagefter flyets bevægelser.<br>


<h4>Kursflyvning</h4>
Eleven skal lære at flyve kursflyvning mod et punkt langt væk eller langs en linje på jorden (vej, jernbane, skovkant osv).<br>
Eleven skal endvidere kunne korrigere kursen ved brug af små bevægelser på side- og krængeror. 
Eleven skal endvidere lære at bruge flyets trim, så flyet selv holder den valgte hastighed, selv om styrepinden slippes<br>
                "
            );
            var lessSPL_A4 = new Models.Training.Training2Lesson(
                "A4",
                @"
<h4>Rorkoordination</h4>
Eleven skal lære at bruge side- og krængeror samtidigt for kunne lave koordinerede drej. Rorenes sekundære virkning skal demonstreres for eleven. <br>
Eleven skal endvidere lære, hvordan uldsnoren eller kuglelibellen kommer ind på plads, så flyve flyver rent. I denne forbindelse skal eleven lære at vurdere, om det den urene flyvning skyldes en forkert krængning eller et forkert udslag på sideroret.<br>

<h4>Krængning og drej</h4>
Ind- og udgang af drej med 20&deg; krængning med korrektion, når krængning er nået. Eleven skal lære at korrigere i drejet<br>
                "
            );
            var lessSPL_A5 = new Models.Training.Training2Lesson(
                "A5",
                @"
<h4>Drej med 30&deg; krængning</h4>
Med udgangspunkt A5 skal eleven øve drej med større krængning på 30&deg; krængning, som skal fastholdes idrejet. Både ind- og udgang i drejet skal være koordineret og rent, og eleven skal kunne flyve 8-taller med 30&deg; krængning.<br>

<h4>Udretning på bestemte kurser</h4>
Eleven skal lære at rette ud mod bestemt terrænpunkt eller på en given kompaskurs. Sideglidning og udskridning i drej demonstreres<br>

<h4>Landingsrunde</h4>
Eleven skal lære at sætte en korrekt landingsrunde op under hensyntagen til vind og anden trafik.<br> 
Eleven skal lære proceduren for udkig i landingsrunden og forstå betydningen af begrebet sigtepunkt i indflyvningen samt begynde at bruge luftbremser til regulering af indflyvning
                "
            );
            var lessSPL_A6 = new Models.Training.Training2Lesson(
                "A6",
                @"
<h4>Forberedelse til spilstart</h4>
Efter at instruktøren har demonstreret spilstart, skal eleven lære at udføre spilstart. Eleven og instruktøren skal på forhånd aftale, i hvilke situationer instruktøren overtager styringen i spilstarten.<br> 
Eleven skal lære kommunikation og signalgivning under spilstart og trænes i, at wiren skal udløses, hvis hastigheden kommer ned på min. hastighed og op på max. hastighed i spilstart.

<h4>Normal spilstart</h4>
Eleven skal fortsætte træningen i spilstart med hovedvægten på starter i nogenlunde direkte modvind. <br>
Eleven skal forberede sig mentalt til en evt. afbrudt start, og eleven skal under cockpitcheck fortælle, hvad hans beslutning er vedr. afbrudt start i forskellige højdebånd.<br>
Eleven skal lære at aflaste wiren, når toppen af spilstarten er ved at være nået, således at wiren ikke er spændt og dermed risikerer at lave overløb i spillet<br>


<h4>Spilstart i sidevind</h4>
Eleven skal fortsætte træningen i spilstart med hovedvægten på starter i sidevind. Eleven skal forberede sig mentalt til en evt. afbrudt start, og eleven skal under cockpitcheck fortælle, hvad hans beslutning er vedr. afbrudt start i forskellige højdebånd.<br>
Eleven skal lære at aflaste wiren, når toppen af spilstarten er ved at være nået, således at wiren ikke er spændt og dermed risikerer at lave overløb i spillet<br>


<h4>Faresituationer i spilstart</h4>
Eleven skal kende faresituationerne i spilstarten og lære at agere på dem. Især bevidstheden om minimum fart i spilstart er helt afgørende. Når denne nås, sænkes næsen og wiren kobles af – uanset højden.<br>
Hvis max. hastighed ispilstart nås, uden at signalering har virket, kobles wiren også af.<br>
Eleven skal trænes i afbrudte starter i forskellige højder. Nogle af disse kan godt ske på senere lektioner, således at nogle afbrudte starter står åbne i elevloggen i nogen tid.<br>


<h4>Landingsrunde - fortsat</h4>
Eleven skal lære at sideglide på kurs i stor højde for at kunne bruge det som landingshjælp.<br>
Eleven skal fortsætte træningen i indflyvning og landing.

                "
            );
            var lessSPL_A7 = new Models.Training.Training2Lesson(
                "A7",
                @"
<h4>Landing</h4>
Eleven skal lære at lave korrekt mærkelanding indenfor +/- 25 meter, og udfladningen skal beherskes selvstændigt.<br>
Eleven skal lære at tage hensyn til vinden og dermed længden af indflyvningen afhængig af vindstyrken.<br>
Eleven skal lære at flyve ind med fuldt luftbremseudslag i en stejl nedflyvning, og eleven skal øve landinger i sidevind<br>


<h4>Kort landing</h4>
Eleven skal lære at landing med kort afløb – teknik ved udelanding. Hvis det er muligt vil denne øvelse med fordel kunne laves et andet sted på flyvepladsen, end hvor landingsfeltet normalt ligger.
                "
            );
            var lessSPL_A8 = new Models.Training.Training2Lesson(
                "A8",
                @"
<h4>Langsomflyvning</h4>
Eleven skal lære at styre flyet ved lav hastighed og stor og kritisk indfaldsvinkel samt erkende symptomer på, at flyet er tæt på stallingsgrænsen.<br>
 Forud for manøvrer som kan medføre stall eller spind er det afgørende at sikre sig, at luftrummet under flyet er frit.

<h4>Stall</h4>
Eleven skal prøve at flyve flyet ind i et stall fra ligeudflyvning for herefter at rette ud ved at få flyvefart på igen ved at trykke flyets næse.<br>
 Han skal endvidere instrueres om proceduren for udretning, hvis den ene vinger dykker i stallet

<h4>Særlige stall</h4>
Eleven skal prøve at stalle, når flyet er i landingskonfiguration med luftbremser ud og evt. flaps udfældet. <br>
Udretning skal også indeholde lukning af luftbremser.<br>
Eleven skal også prøve stall under stor G-påvirkning – f.eks. med stor krængning eller high-speed-stall
"
            );
            var lessSPL_A9 = new Models.Training.Training2Lesson(
                "A9",
                @"
<h4>Drej med stor krængning</h4>
Eleven skal lære drej med 45&deg; krængning samt prøve og rette ud fra unormale flyvestillinger.<br>
 Eleven skal også prøve at gå ind i spiraldyk samt rette ud af denne ved først at bruge krængeror og herefter højderor.<br>

<h4>Forebyggelse af spind</h4>
Eleven skal prøve et stall under drej samt lære at rette ud af det.<br>
 Proceduren er den samme som ved udretning af spind (modsat sideror – kort pause – pinden lidt frem – ret ud af dykket).<br>
 Eleven skal endvidere lære at undgå, at et stall under drej udvikler sig til et egentligt spind.
                "
            );
            var lessSPL_A10 = new Models.Training.Training2Lesson(
                "A10",
                @"
<h4>Begyndende spind</h4>
Eleven skal prøve at gå ind i et spind fra et stall under drej, der udvikler sig fra at den ene vinge dykker. <br>
Eleven skal lære at rette ud fra spindet.<br>
Hvis uddannelsesstedet ikke har et skolefly, der kan spinde, bør denne øvelse foregå i et fly, der kan spinde.<br>

<h4>Udviklet spind</h4>
Eleven skal prøve et fuldt udviklet spind på mindst én hel omgang.<br>
Eleven skal kunne håndtere den stressede situation, der kan opstå under denne manøvre.<br>
Hvis uddannelsesstedet ikke har et skolefly, der kan spinde, bør denne øvelse foregå i et fly, der kan spinde.                "
            );
            var lessSPL_A11 = new Models.Training.Training2Lesson(
                "A11",
                @"
<h4>Opsamling før solo</h4>
Eleven skal selvstændigt demonstrere dagligt tilsyn på skoleflyet og udfylde omskolingsskema til flyet med udgangspunkt i flyets håndbog. Eleven skal redegøre for cockpitcheck, som skal udføres under soloflyvning <br>

<h4>Programflyvninger</h4>
Eleven skal flyve et program, som instruktøren har givet forud for flyvningen. Hvis programmet indeholder flere emner, end flyvetiden tillader, skal eleven selvstændigt afbryde programmet for at kunne udføre en korrekt landingsrunde og landing.<br>

<h4>Visuel reference</h4>
Eleven skal flyve en flyvning med tildækket højdemåler, hvor eleven alene ud fra visuelle referencer gennemfører en landingsrunde og mærkelanding uden at kunne bruge højdemåleren.<br>

<h4>Gennemgang af relevant teori</h4>
Eleven skal gennemgå relevant teori med emner fra fagene i SPL-teorien, hvis ikke eleven i forvejen har bestået teorien til SPL-certifikatet. Teorien skal indeholde relevante emner, som vedrører den konkrete flyveplads, som soloflyvningen skal foregå fra – f.eks. luftrum, klarering, miljøbestemmelser osv."
            );
            var lessSPL_A12 = new Models.Training.Training2Lesson(
                "A12",
                @"
<h4>Soloflyvning</h4>
Eleven skal gennemføre min. tre soloflyvninger på skoleflyet
                "
            );
            var lessSPL_B1 = new Models.Training.Training2Lesson(
                "B1",
                @"
<h4>Typeomskoling - forberedelse</h4>
Eleven skal forberede omskolingen til et ensædet svævefly, som kan bruges til resten af den soloflyvning, der skal til for at få et SPL-certifikat.<br>
Forberedelsen omfatter gennemgang flyets håndbog med udfyldelse af omskolingsskema, instruktion om indretning af cockpit samt adskillelse og samling af det pågældende svævefly<br>

<h4>Typeomskoling – praktisk flyvning</h4>
Eleven skal gennemgå den praktiske omskoling til flyet incl. flyvning med lav og høj hastighed, stall fra ligeudflyvning, betjening af optrækkeligt understel
                "
            );
            var lessSPL_B2 = new Models.Training.Training2Lesson(
                "B2",
                @"
<h4>Soloflyvning på en-sædet svævefly</h4>
Eleven skal min. have 2 timers soloflyvning, og denne lektion giver mulighed for at opbygge rutine på flyet til opnåelse af disse to timer og til evt. at flyve solostrækflyvning som en senere del af uddannelsen til SPL-certifikatet. <br>
I denne lektion får eleven lejlighed til at prøve det ensædede svævefly i forskellige vejrforhold                "
            );
            var lessSPL_B3 = new Models.Training.Training2Lesson(
                "B3",
                @"
<h4>Begyndende termikflyvning</h4>
Eleven skal lære at flyve i termikken, og øvelsen kan gennemføres på både to-sædet og en-sædet svævefly. <br>
I starten af øvelsen vil det være hensigtsmæssigt, hvis eleven får lejlighed til at træne uden at skulle ligge i opvindsområder med mange andre svævefly<br>

                "
            );
            var lessSPL_B4 = new Models.Training.Training2Lesson(
                "B4",
                @"
<h4>Videregående termikflyvning og anden trafik</h4>
Eleven skal lære at flyve i termikken, og øvelsen kan gennemføres på både to-sædet og en-sædet svævefly. <br>
I denne øvelse vil det være hensigtsmæssigt, hvis eleven får lejlighed til at træne flyvning, hvor han også skal holde øje med andre svævefly i opvindsområdet<br>

                "
            );
            var lessSPL_B5 = new Models.Training.Training2Lesson(
                "B5",
                @"
<h4>Udelanding</h4>
Eleven skal lære at udpege egnede marker til udelanding, vurdere indflyvningsforhold samt vurdere markens overflade og hældning.<br>
Eleven skal med udgangspunkt i den engelske landingsrunde lære at sætte en landingsrunde op til den mark, som han vil bruge.<br>
Højden skal vurderes med den vinkel, som eleven ser marken i.<br>
I denne øvelse bør landing på en anden flyveplads indgå.<br>
Det er oplagt at bruge en TMG til denne øvelse, da øvelsen kan gentages igen og igen, indtil resultatet er tilfredsstillende. <br>
</i>Denne øvelse kan kun ske i to-sædet fly med instruktør</i>. <br>
<h4>Bemærk:</h4>
Landingsøvelser til marker skal afbrydes i min. 500’ AGL, medmindre der foreligger en skriftlig aftale med markens ejer om, at marken må bruges til landingsøvelser.<br>
Hvis en sådan tilladelse foreligger, skal indflyvningen afbrydes senest i 50’ (15 meter) AGL.
                "
            );
            var lessSPL_B6 = new Models.Training.Training2Lesson(
                "B6",
                @"
<h4>Planlægning af strækflyvning</h4>
Eleven skal i denne lektion selvstændigt, men under overværelse af en instruktør, planlægge en strækflyvning, som skal være på min. 50 km, hvis den flyves solo, og min. 100 km hvis den flyves med instruktør i et to-sædet svævefly.<br>
Øvelsen i to-sædet svævefly med instruktør må alternativt foregå i en TMG, men flyvningen skal gennemføres således, at den svarer til flyvning i et svævefly. Den praktiske udførelse fremgår af B7.<br>
                "
            )
            {Precondition = @"Alle deløvelser i denne lektion skal være gennemgået, inden eleven flyver strækflyvning solo eller med instruktør" };
            var lessSPL_B7 = new Models.Training.Training2Lesson(
                "B7",
                @"
<h4>Strækflyvning og optimering</h4>
Eleven skal i denne lektion demonstrere, at han under flyvningen kan håndtere de situationer, som opstår, og udføre nødvendige tiltag i den udstrækning, der bliver behov for det.<br>
 Eleven skal samtidig kunne navigere på ruten og sikre sig klarering ved flyvekontrollen i den udstrækning, der er behov for det.<br>
Opstår der usikkerhed om flyets position, skal eleven kunne iværksætte sådanne tiltag, at sikkerhed om positionen igen opnås.<br>
Ved landing på en anden flyveplads skal eleven kunne placere sig i landingsrunden og i forhold til anden trafik samt lande sikkert på den fremmede flyveplads.<br>
Hvis flyvningen flyves med instruktør, skal distancen er være min. 100 km.<br>
Hvis flyvningen foregår i TMG, skal den tilrettelægges således, at flyvningen kommer til at ligne en flyvning i svævefly – dvs. med stigning med motor en en given højde, hvorefter flyvningen fortsætter med motoren i tomgang. 
Hvis der ikke er termik, kan denne erstattes af stigning med motor, indtil højde er nået til næste del af strækflyvningen. Motoren må betjenes af instruktøren under flyvningen.
                "
            )
            {Precondition= @"Alle deløvelser i lektion 43 skal være gennemgået, inden eleven flyver strækflyvning solo eller med instruktør. Hvis
strækflyvningen flyves solo, skal distancen være min. 50 km, og instruktøren skal autorisere eleven til denne flyvning.
" };
            var lessSPL_B8 = new Models.Training.Training2Lesson(
                "B8",
                @"
<h4>Forberedelse til certifikatprøve</h4>
Efter gennemførelse af nogle flyvninger under denne lektion skal instruktøren kunne indstille eleven til prøve hos en eksaminator.<br>
 Eleven skal kunne blive indstillet ved at kunne udføre øvelserne indenfor de angivne tolerancer, der er beskrevet nedenfor.                "
            )
            {Precondition= @"
<ul>
<li>Eleven skal være fyldt 16 år</li>
<li>Uddannelsesprogram fra A-0 til B-7 skal være gennemført</li>
<li>Eleven skal have mindst 15 timers flyvetid på svævefly, heraf:
  <ul>
    <li>min. 10 timers to-sædet instruktion</li>
    <li>min. 2 timers soloflyvning</li>
  </ul>
</li>

<li>Eleven skal have mindst 45 starter</li>
<li>Min. én solostrækflyvning på 50 km eller mindst én to-sædet strækflyvning på 100 km med instruktør</li>
<li>Eleven skal have gennemgået og bestået teorien til SPL-certifikat</li>
</ul>

Kravet om min. 15 timers samlet flyvetid som elev kan reduceres til min. 7,5 timer<br>
Kravet om flyvetid solo på svævefly på min. 2 timer kan ikke reduceres<br>
Krav om strækflyvning kan ikke reduceres eller erstattes<br>
" };
            ;

            var exSPL_dummy = new Training2Exercise("(placeholder)");
            var exSPL_A0_1 = new Training2Exercise("Svæveflyets karakteristika", true);
            var exSPL_A0_2 = new Training2Exercise("Cockpit, instrumenter og udstyr", true);
            var exSPL_A0_3 = new Training2Exercise("Styregrejer, håndtag og kontakter", true);
            var exSPL_A0_4 = new Training2Exercise("Checkliste, procedure og kontrol", true);
            var exSPL_A0_5 = new Training2Exercise("Nødkast af førerskærm og funktion af faldskærm", true){Note= @"f.eks. blokeret ror, luftbremse, udløser til kobling osv. " };
            var exSPL_A0_6 = new Training2Exercise("Systemsvigt – hvad gør man?", true);
            var exSPL_A0_7 = new Training2Exercise("Procedure for udspring", true);
            var exSPL_A0_8 = new Training2Exercise("DSvU’s Safety Management System", true){Note= @"Grundlag for at øge den fremtidige flyvesikkerhed" };
            var exSPL_A0_9 = new Training2Exercise("Rapportering af hændelser m.v.", true){Note= @"Også hvor der ikke skete noget, men hvor risikoen var der" };
            var exSPL_A0_10 = new Training2Exercise("Hvordan rapporterer man", true){Note= @"Via DSvU’s hjemmeside" };

            var exSPL_A1_1 = new Training2Exercise("Briefing inden flyvning", true){Note= @"Instruktøren har forud kigget i elevens uddannelseslog." };
            var exSPL_A1_2 = new Training2Exercise("Dokumenter til rådighed", true){Note= @"Flyets dokumenter mv" };
            var exSPL_A1_3 = new Training2Exercise("Nødvendigt udstyr", true);
            var exSPL_A1_4 = new Training2Exercise("Håndtering af fly før flyvning");
            var exSPL_A1_5 = new Training2Exercise("Tilsyn og kontrol med fly");
            var exSPL_A1_6 = new Training2Exercise("Max. og min. vægt / tyngdepunkt"){Note= @"Evt. ballast ved elev med lav vægt" };
            var exSPL_A1_7 = new Training2Exercise("Indstilling af sæde, pedaler og seler");
            var exSPL_A1_8 = new Training2Exercise("Cockpitcheck efter checkliste");
            var exSPL_A1_9 = new Training2Exercise("Hvad er flyets mindste hastighed i spilstart") { Note = @"Udkobling hvis min. hastighed nås " };
            var exSPL_A1_10 = new Training2Exercise("Hvad er flyets maksimale hastighed i spilstart") { Note = @"Udkobling hvis max. hastighed nås " };
            var exSPL_A1_11 = new Training2Exercise("Forberedelse til afbrudt start"){Note= @"Eleven lærer, at han fremover skal fortælle om sine overvejelser om en evt. afbrudt start" };
            var exSPL_A1_12 = new Training2Exercise("Orientering fra luften"){Note= @"Markante terrænpunkter" };
            var exSPL_A1_13 = new Training2Exercise("Procedure for udkig");

            tpSPL_S.Lessons = new[]
            {
                lessSPL_A0,
                lessSPL_A1,
                lessSPL_A2,
                lessSPL_A3,
                lessSPL_A4,
                lessSPL_A5,
                lessSPL_A6,
                lessSPL_A7,
                lessSPL_A8,
                lessSPL_A9,
                lessSPL_A10,
                lessSPL_A11,
                lessSPL_A12,
                lessSPL_B1,
                lessSPL_B2,
                lessSPL_B3,
                lessSPL_B4,
                lessSPL_B5,
                lessSPL_B6,
                lessSPL_B7,
                lessSPL_B8
            };
                
                lessSPL_A0.Exercises = new[]{                
                    exSPL_A0_1,
                    exSPL_A0_2,
                    exSPL_A0_3,
                    exSPL_A0_4,
                    exSPL_A0_5,
                    exSPL_A0_6,
                    exSPL_A0_7,
                    exSPL_A0_8,
                    exSPL_A0_9,
                    exSPL_A0_10,
                };

                lessSPL_A1.Exercises = new[] {                
                    exSPL_A1_1,
                    exSPL_A1_2,
                    exSPL_A1_3,
                    exSPL_A1_4,
                    exSPL_A1_5,
                    exSPL_A1_6,
                    exSPL_A1_7,
                    exSPL_A1_8,
                    exSPL_A1_9,
                    exSPL_A1_10,
                    exSPL_A1_11,
                    exSPL_A1_12,
                    exSPL_A1_13
                };
            context.TrainingPrograms.AddOrUpdate(
                tpSPL_S
            );
            lessSPL_A2.Exercises = new[] {exSPL_dummy};
            lessSPL_A3.Exercises = new[] {exSPL_dummy};
            lessSPL_A4.Exercises = new[] {exSPL_dummy};
            lessSPL_A5.Exercises = new[] {exSPL_dummy};
            lessSPL_A6.Exercises = new[] {exSPL_dummy};
            lessSPL_A7.Exercises = new[] {exSPL_dummy};
            lessSPL_A8.Exercises = new[] {exSPL_dummy};
            lessSPL_A9.Exercises = new[] {exSPL_dummy};
            lessSPL_A10.Exercises = new[] {exSPL_dummy};
            lessSPL_A11.Exercises = new[] {exSPL_dummy};
            lessSPL_A12.Exercises = new[] {exSPL_dummy};
            lessSPL_B1.Exercises = new[] {exSPL_dummy};
            lessSPL_B2.Exercises = new[] {exSPL_dummy};
            lessSPL_B3.Exercises = new[] {exSPL_dummy};
            lessSPL_B4.Exercises = new[] {exSPL_dummy};
            lessSPL_B5.Exercises = new[] {exSPL_dummy};
            lessSPL_B6.Exercises = new[] {exSPL_dummy};
            lessSPL_B7.Exercises = new[] {exSPL_dummy};
            lessSPL_B8.Exercises = new[] {exSPL_dummy};


            context.SaveChanges();
        }
    }
}