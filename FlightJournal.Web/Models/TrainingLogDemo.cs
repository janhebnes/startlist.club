using System.Collections.Generic;

namespace FlightJournal.Web.Models
{
    public class TrainingLogDemo
    {
        //    public static TrainingProgramViewModel BuildSplWinchTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "spl-winch",
        //            Name = "SPL-spilstart",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A0",
        //                    Id = "A0",
        //                    Description = "Grundlæggende kendskab til svævefly og flyvesikkerhed",
        //                    Exercises = new List<TrainingExerciseViewModel>
        //                    {
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "1",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Svæveflyets karakteristika",
        //                            Status = TrainingStatus.Briefed
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "2",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Cockpitindretning, instrumenter og udstyr"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "3",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Kontakter, styrepind, pedaler, luftbremser, trim og evt.flaps"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "4",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Wireudløser og optrækkeligt understel"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "5",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Checkliste, procedurer og kontroller"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "6",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Brug af sikkerhedsudstyr(nødafkast og faldskærm)"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "7",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Hvad skal man gøre ved systemsvigt?"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "8",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Procedure for udspring"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "9",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Safety Management System"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "10",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Rapporter alle hændelser med risici, som kunne have udviklet sig til uheld, havarier el.lign."
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "11",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Hvordan indberetter man – blanketter osv"
        //                        }
        //                    }
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A1",
        //                    Id = "A1",
        //                    Description = "Tilvænningsflyvning",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A2",
        //                    Id = "A2",
        //                    Description = "Fartkontrol og brug af ror"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A3",
        //                    Id = "A3",
        //                    Description = "Ligeudflyvning og kursflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A4",
        //                    Id = "A4",
        //                    Description = "Krængning og drej"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A5",
        //                    Id = "A5",
        //                    Description = "Drej og kombinationsflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A6",
        //                    Id = "A6",
        //                    Description = "Start",
        //                    Exercises = new List<TrainingExerciseViewModel>
        //                    {
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "1",
        //                            Description = "Aftale nødprocedure under start",
        //                            BriefingOnlyRequired = true
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "2",
        //                            Description = "Signalgivning og kommunikation før og under start",
        //                            Status = TrainingStatus.Completed
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "3",
        //                            Description = "Brug af udstyr ifm. spilstarten",
        //                            Status = TrainingStatus.Briefed
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "4",
        //                            Description = "Kontrol før start",
        //                            Status = TrainingStatus.Trained
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "5",
        //                            Description = "Beslutning om reaktion ved afbrudt start i forskellige højder",
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "6",
        //                            Description = "Start i direkte modvind",
        //                            Status = TrainingStatus.Trained,
        //                            LongDescription = "Eleven skal fortsætte træningen i spilstart med hovedvægten på starter i nogenlunde direkte modvind. Eleven skal forberede sig mentalt til en evt. afbrudt start, og eleven skal under cockpitcheck fortælle, hvad hans beslutning er vedr. afbrudt start i forskellige højdebånd. Eleven skal lære at aflaste wiren, når toppen af spilstarten er ved at være nået, således at wiren ikke er spændt og dermed risikerer at lave overløb i spillet"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "7",
        //                            Description = "Start i sidevind",
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "8",
        //                            Description = "Den optimale profil for en spilstart samt begrænsninger",
        //                        },
        //                        // etc, etc
        //                    }
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A7",
        //                    Id = "A7",
        //                    Description = "Landing",
        //                    Exercises = new List<TrainingExerciseViewModel>
        //                    {
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "1",
        //                            Description = "Procedure for at gå ind i landingsrunden",
        //                            Status = TrainingStatus.Briefed
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "2",
        //                            Description = "Procedure og teknik for udkig i landingsrunden",
        //                            Status = TrainingStatus.Briefed
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "3",
        //                            Description = "Check før landing, observationslinje, anflyvning, indflyvning",
        //                        },

        //                    }
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A8",
        //                    Id = "A8",
        //                    Description = "Langsomflyvning og stall"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A9",
        //                    Id = "A9",
        //                    Description = "Specielle drej, stall i drej og forebyggelse af spind i et drej"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A10",
        //                    Id = "A10",
        //                    Description = "Spin, erkendelse or forebyggelse"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A11",
        //                    Id = "A11",
        //                    Description = "Forberedelse til soloflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A12",
        //                    Id = "A12",
        //                    Description = "Soloflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B1",
        //                    Id = "B1",
        //                    Description = "Omskoling til en-sædet svævefly"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B2",
        //                    Id = "B2",
        //                    Description = "Landingsøvelser under forskellige forhold"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B3",
        //                    Id = "B3",
        //                    Description = "Begyndende termikflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B4",
        //                    Id = "B4",
        //                    Description = "Termikflyvning og anden trafik"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B5",
        //                    Id = "B5",
        //                    Description = "Udelanding"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B6",
        //                    Id = "B6",
        //                    Description = "Planlægning af strækflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B7",
        //                    Id = "B7",
        //                    Description = "Navigation og optimering"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B8",
        //                    Id = "B8",
        //                    Description = "Forberedelse til certifikatprøve"
        //                },
        //            }
        //        };
        //        return program;
        //    }
        //    public static TrainingProgramViewModel BuildSplTowTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "spl-tow",
        //            Name = "SPL-flyslæb",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A0",
        //                    Id = "A0",
        //                    Description = "Grundlæggende kendskab til svævefly og flyvesikkerhed",
        //                    Exercises = new List<TrainingExerciseViewModel>
        //                    {
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "1",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Svæveflyets karakteristika"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "2",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Cockpitindretning, instrumenter og udstyr"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "3",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Kontakter, styrepind, pedaler, luftbremser, trim og evt.flaps"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "4",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Wireudløser og optrækkeligt understel"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "5",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Checkliste, procedurer og kontroller"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "6",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Brug af sikkerhedsudstyr(nødafkast og faldskærm)"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "7",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Hvad skal man gøre ved systemsvigt?"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "8",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Procedure for udspring"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "9",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Safety Management System"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "10",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Rapporter alle hændelser med risici, som kunne have udviklet sig til uheld, havarier el.lign."
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "11",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Hvordan indberetter man – blanketter osv"
        //                        }
        //                    }
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A1",
        //                    Id = "A1",
        //                    Description = "Tilvænningsflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A2",
        //                    Id = "A2",
        //                    Description = "Fartkontrol og brug af ror"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A3",
        //                    Id = "A3",
        //                    Description = "Ligeudflyvning og kursflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A4",
        //                    Id = "A4",
        //                    Description = "Krængning og drej"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A5",
        //                    Id = "A5",
        //                    Description = "Drej og kombinationsflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A6",
        //                    Id = "A6",
        //                    Description = "Start"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A7",
        //                    Id = "A7",
        //                    Description = "Landing"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A8",
        //                    Id = "A8",
        //                    Description = "Langsomflyvning og stall"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A9",
        //                    Id = "A9",
        //                    Description = "Specielle drej, stall i drej og forebyggelse af spind i et drej"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A10",
        //                    Id = "A10",
        //                    Description = "Spin, erkendelse or forebyggelse"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A11",
        //                    Id = "A11",
        //                    Description = "Forberedelse til soloflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A12",
        //                    Id = "A12",
        //                    Description = "Soloflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B1",
        //                    Id = "B1",
        //                    Description = "Omskoling til en-sædet svævefly"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B2",
        //                    Id = "B2",
        //                    Description = "Landingsøvelser under forskellige forhold"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B3",
        //                    Id = "B3",
        //                    Description = "Begyndende termikflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B4",
        //                    Id = "B4",
        //                    Description = "Termikflyvning og anden trafik"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B5",
        //                    Id = "B5",
        //                    Description = "Udelanding"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B6",
        //                    Id = "B6",
        //                    Description = "Planlægning af strækflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B7",
        //                    Id = "B7",
        //                    Description = "Navigation og optimering"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B8",
        //                    Id = "B8",
        //                    Description = "Forberedelse til certifikatprøve"
        //                },
        //            }
        //        };
        //        return program;
        //    }
        //    public static TrainingProgramViewModel BuildSplTmgTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "spl-tmg",
        //            Name = "SPL-TMG",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A0",
        //                    Id = "A0",
        //                    Description = "Grundlæggende kendskab til svævefly og flyvesikkerhed",
        //                    Exercises = new List<TrainingExerciseViewModel>
        //                    {
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "1",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Svæveflyets karakteristika"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "2",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Cockpitindretning, instrumenter og udstyr"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "3",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Kontakter, styrepind, pedaler, luftbremser, trim og evt.flaps"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "4",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Wireudløser og optrækkeligt understel"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "5",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Checkliste, procedurer og kontroller"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "6",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Brug af sikkerhedsudstyr(nødafkast og faldskærm)"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "7",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Hvad skal man gøre ved systemsvigt?"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "8",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Procedure for udspring"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "9",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Safety Management System"
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "10",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Rapporter alle hændelser med risici, som kunne have udviklet sig til uheld, havarier el.lign."
        //                        },
        //                        new TrainingExerciseViewModel
        //                        {
        //                            Id = "11",
        //                            BriefingOnlyRequired = true,
        //                            Description = "Hvordan indberetter man – blanketter osv"
        //                        }
        //                    }
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A1",
        //                    Id = "A1",
        //                    Description = "Tilvænningsflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A2",
        //                    Id = "A2",
        //                    Description = "Fartkontrol og brug af ror"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A3",
        //                    Id = "A3",
        //                    Description = "Ligeudflyvning og kursflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A4",
        //                    Id = "A4",
        //                    Description = "Krængning og drej"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A5",
        //                    Id = "A5",
        //                    Description = "Drej og kombinationsflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A6",
        //                    Id = "A6",
        //                    Description = "Start"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A7",
        //                    Id = "A7",
        //                    Description = "Landing"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A8",
        //                    Id = "A8",
        //                    Description = "Langsomflyvning og stall"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A9",
        //                    Id = "A9",
        //                    Description = "Specielle drej, stall i drej og forebyggelse af spind i et drej"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A10",
        //                    Id = "A10",
        //                    Description = "Spin, erkendelse or forebyggelse"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A11",
        //                    Id = "A11",
        //                    Description = "Forberedelse til soloflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "A12",
        //                    Id = "A12",
        //                    Description = "Soloflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B1",
        //                    Id = "B1",
        //                    Description = "Omskoling til en-sædet svævefly"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B2",
        //                    Id = "B2",
        //                    Description = "Landingsøvelser under forskellige forhold"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B3",
        //                    Id = "B3",
        //                    Description = "Begyndende termikflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B4",
        //                    Id = "B4",
        //                    Description = "Termikflyvning og anden trafik"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B5",
        //                    Id = "B5",
        //                    Description = "Udelanding"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B6",
        //                    Id = "B6",
        //                    Description = "Planlægning af strækflyvning"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B7",
        //                    Id = "B7",
        //                    Description = "Navigation og optimering"
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "B8",
        //                    Id = "B8",
        //                    Description = "Forberedelse til certifikatprøve"
        //                },
        //            }
        //        };
        //        return program;
        //    }
        //    public static TrainingProgramViewModel BuildStartMethodWinchTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "startmethod-winch",
        //            Name = "Startmetode spil",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS1",
        //                    Id = "SS1",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS2",
        //                    Id = "SS2",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS3",
        //                    Id = "SS3",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS4",
        //                    Id = "SS4",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS5",
        //                    Id = "SS5",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS6",
        //                    Id = "SS6",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS7",
        //                    Id = "SS7",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS8",
        //                    Id = "SS8",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS9",
        //                    Id = "SS9",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS10",
        //                    Id = "SS10",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS11",
        //                    Id = "SS11",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS12",
        //                    Id = "SS12",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SS13",
        //                    Id = "SS12",
        //                    Description = "",
        //                }
        //            }
        //        };
        //        return program;
        //    }
        //    public static TrainingProgramViewModel BuildStartMethodTowTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "startmethod-tow",
        //            Name = "Startmetode flyslæb",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF1",
        //                    Id = "SF1",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF2",
        //                    Id = "SF2",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF3",
        //                    Id = "SF3",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF4",
        //                    Id = "SF4",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF5",
        //                    Id = "SF5",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF6",
        //                    Id = "SF6",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF7",
        //                    Id = "SF7",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF8",
        //                    Id = "SF8",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF9",
        //                    Id = "SF9",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF10",
        //                    Id = "SF10",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "SF11",
        //                    Id = "SF11",
        //                    Description = "",
        //                }
        //            }
        //        };
        //        return program;
        //    }
        //    public static TrainingProgramViewModel BuildTypeRatingSingleTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "typerating-single",
        //            Name = "Typeomskoling ensædet",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T1",
        //                    Id = "T1",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T2",
        //                    Id = "T2",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T3",
        //                    Id = "T3",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T4",
        //                    Id = "T4",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T5",
        //                    Id = "T5",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T6",
        //                    Id = "T6",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T7",
        //                    Id = "T7",
        //                    Description = "",
        //                }
        //            }
        //        };
        //        return program;
        //    }
        //    public static TrainingProgramViewModel BuildTypeRatingDualTrainingProgram()
        //    {
        //        var program = new TrainingProgramViewModel
        //        {
        //            Id = "typerating-dual",
        //            Name = "Typeomskoling tosædet",
        //            Lessons = new List<TrainingLessonViewModel>
        //            {
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T1",
        //                    Id = "T1",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T2",
        //                    Id = "T2",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T3",
        //                    Id = "T3",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T4",
        //                    Id = "T4",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T5",
        //                    Id = "T5",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T6",
        //                    Id = "T6",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T7",
        //                    Id = "T7",
        //                    Description = "",
        //                },
        //                new TrainingLessonViewModel
        //                {
        //                    Name = "T8",
        //                    Id = "T8",
        //                    Description = "",
        //                }
        //            }
        //        };
        //        return program;
        //    }
    }
}