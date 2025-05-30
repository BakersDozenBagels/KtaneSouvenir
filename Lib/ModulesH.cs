﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Souvenir;
using UnityEngine;
using Rnd = UnityEngine.Random;

public partial class SouvenirModule
{
    private IEnumerator<YieldInstruction> ProcessH(ModuleData module)
    {
        var comp = GetComponent(module, "HexOS");
        yield return WaitForSolve;

        var answer = ((char) ('A' + GetIntField(comp, "WhatToSubmit").Get(min: 0, max: 25))).ToString();
        addQuestion(module, Question.HLetter, correctAnswers: new[] { answer });
    }

    private IEnumerator<YieldInstruction> ProcessHalliGalli(ModuleData module)
    {
        var comp = GetComponent(module, "halliGalli");
        var bell = GetField<KMSelectable>(comp, "bell", true).Get();
        var stage = GetIntField(comp, "stage");
        int fruit = -1;
        string figure = "";

        var oldInteract = bell.OnInteract;
        bell.OnInteract = () =>
        {
            if (stage.Get(min: 0, max: 2) != 1) return oldInteract();

            var fruits = GetArrayField<int>(comp, "displayedFruits").Get(expectedLength: 3);
            var counts = GetArrayField<int>(comp, "displayedCounts").Get(expectedLength: 3);

            fruit = -1;
            for (int i = 0; i < 5; i++)
                if (Enumerable.Range(0, 3).Where(j => fruits[j] == i).Select(j => counts[j]).Sum() == 5)
                    fruit = i;
            var contrib = new List<int>(3);
            for (int i = 0; i < 3; i++)
                if (fruits[i] == fruit)
                    contrib.Add(counts[i]);
            figure = contrib.OrderBy(x => x).JoinString(" ");

            return oldInteract();
        };

        yield return WaitForSolve;

        if (fruit == -1 || figure == "")
            throw new AbandonModuleException($"The solution was somehow missed. (fruit={fruit}, figure={figure})");

        addQuestions(module,
            makeQuestion(Question.HalliGalliFruit, module, correctAnswers: new[] { Question.HalliGalliFruit.GetAnswers()[fruit] }),
            makeQuestion(Question.HalliGalliCounts, module, correctAnswers: new[] { figure }));
    }

    private IEnumerator<YieldInstruction> ProcessHereditaryBaseNotation(ModuleData module)
    {
        var comp = GetComponent(module, "hereditaryBaseNotationScript");
        var mthNumberToBaseNString = GetMethod<string>(comp, "numberToBaseNString", numParameters: 2);

        yield return WaitForSolve;

        var baseN = GetIntField(comp, "baseN").Get(3, 7);
        var upperBound = new[] { 19682, 60000, 80000, 100000, 100000 }[baseN - 3];
        var initialNum = GetIntField(comp, "initialNumber").Get(1, upperBound);

        var answer = mthNumberToBaseNString.Invoke(baseN, initialNum).ToString();
        var invalidAnswer = new HashSet<string> { answer };

        // Generate fake options in the same base of the answer
        while (invalidAnswer.Count() < 4)
            invalidAnswer.Add(mthNumberToBaseNString.Invoke(baseN, Rnd.Range(1, upperBound + 1)).ToString());

        addQuestion(module, Question.HereditaryBaseNotationInitialNumber, correctAnswers: new[] { answer }, preferredWrongAnswers: invalidAnswer.ToArray());
    }

    private IEnumerator<YieldInstruction> ProcessHexabutton(ModuleData module)
    {
        var comp = GetComponent(module, "hexabuttonScript");
        var labels = GetArrayField<string>(comp, "labels").Get();
        var index = GetIntField(comp, "labelNum").Get(0, labels.Length - 1);

        yield return WaitForSolve;
        addQuestion(module, Question.HexabuttonLabel, correctAnswers: new[] { labels[index] });
    }

    private IEnumerator<YieldInstruction> ProcessHexamaze(ModuleData module)
    {
        var comp = GetComponent(module, "HexamazeModule");
        yield return WaitForSolve;
        addQuestion(module, Question.HexamazePawnColor, correctAnswers: new[] { new[] { "Red", "Yellow", "Green", "Cyan", "Blue", "Pink" }[GetIntField(comp, "_pawnColor").Get(0, 5)] });
    }

    private IEnumerator<YieldInstruction> ProcessHexOrbits(ModuleData module)
    {
        var comp = GetComponent(module, "HexOrbitsScript");
        yield return WaitForSolve;

        var stages = GetArrayField<int>(comp, "stageValues").Get(expectedLength: 5, validator: v => v is < 0 or > 15 ? $"Bad stage value {v}" : null);
        var shapes = new[] { "Square", "Pentagon", "Hexagon", "Heptagon" };
        addQuestions(module, stages.Take(4).SelectMany((s, i) => new[] {
            makeQuestion(Question.HexOrbitsShape, module, formatArgs: new[] { "slow", Ordinal(i + 1) }, correctAnswers: new[] { shapes[s / 4] }),
            makeQuestion(Question.HexOrbitsShape, module, formatArgs: new[] { "fast", Ordinal(i + 1) }, correctAnswers: new[] { shapes[s % 4] })
        }));
    }

    private IEnumerator<YieldInstruction> ProcessHexOS(ModuleData module)
    {
        var comp = GetComponent(module, "HexOS");
        yield return WaitForSolve;

        const string validCharacters = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string[] validPhrases = new string[24] { "a maze with edges like their knives", "someday ill be the shape they want me to be", "but i dont know how much more theyll wake away before theyre satisfied", "they have sliced away my flesh", "shorn of unsightly limbs and organs", "more stitch and scar than human", "if only marble", "grew back so quickly", "they have stolen away my spirit", "memories scattered into the slipstream", "i have no idea who i used to be", "i can only guess", "what they will make me", "they found me in my lowest days", "breathed life back into my frozen body", "promising a more beautiful future", "then i discovered", "what they really wanted", "they pulled me into their vortex", "and i saw my future reflected in their eyes", "a shimmering halo of impossible dreams", "void of my self", "it was", "perfect" };

        var octOS = GetField<bool>(comp, "solvedInOctOS").Get();
        var decipher = GetField<char[]>(comp, "decipher").Get(arr => arr.Length != 2 && arr.Length != 6 ? "expected length 2 or 6" : arr.Any(ch => !validCharacters.Contains(char.ToUpperInvariant(ch))) ? "expected characters A–Z or space" : null);
        var screen = GetField<string>(comp, "screen").Get(s => s.Length != 30 ? "expected length 30" : s.Any(ch => !char.IsDigit(ch)) ? "expected only digits" : null);
        var sum = GetField<string>(comp, "sum").Get(s => s.Length != 4 ? "expected length 4" : s.Any(ch => ch != '0' && ch != '1' && ch != '2' && ch != '3') ? "expected only characters 0–3" : null);

        var qs = new List<QandA>();
        var cipherWrongAnswers = octOS ? validPhrases.SelectMany(str => Enumerable.Range(0, str.Length - 6).Select(ix => str.Substring(ix, 6))).ToArray() : validCharacters.SelectMany(c1 => validCharacters.Select(c2 => string.Concat(c1, c2))).ToArray();

        var wrongAnswers = octOS
            // Generate every combination of 0, 1, 2, & 3 so long as the left two numbers don’t match the right (3031 is valid but 3131 is not)
            ? Enumerable.Range(0, 256).Where(i => i / 16 != i % 16).Select(i => new[] { i / 64, (i / 16) % 4, (i / 4) % 4, i % 4 }.JoinString()).ToArray()
            // Generate every combination of 0, 1, & 2 so long as the left two numbers don’t match the right (2021 is valid but 2121 is not)
            : Enumerable.Range(0, 81).Where(i => i / 9 != i % 9).Select(i => new[] { i / 27, (i / 9) % 3, (i / 3) % 3, i % 3 }.JoinString()).ToArray();

        qs.Add(octOS
            ? makeQuestion(Question.HexOSOctCipher, module, correctAnswers: new[] { decipher.JoinString() }, preferredWrongAnswers: cipherWrongAnswers)
            : makeQuestion(Question.HexOSCipher, module, correctAnswers: new[] { decipher.JoinString(), decipher.Reverse().JoinString() }, preferredWrongAnswers: cipherWrongAnswers));
        qs.Add(makeQuestion(Question.HexOSSum, module, correctAnswers: new[] { sum }, preferredWrongAnswers: wrongAnswers));
        for (var offset = 0; offset < 10; offset++)
            qs.Add(makeQuestion(Question.HexOSScreen, module, formatArgs: new[] { Ordinal(offset + 1) }, correctAnswers: new[] { screen.Substring(offset * 3, 3) }));
        addQuestions(module, qs);
    }

    private IEnumerator<YieldInstruction> ProcessHiddenColors(ModuleData module)
    {
        var comp = GetComponent(module, "HiddenColorsScript");

        var ledcolors = new[] { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Magenta", "White" };
        var ledcolor = GetIntField(comp, "LEDColor").Get(min: 0, max: 7);
        var colors = GetArrayField<Material>(comp, "buttonColors", isPublic: true).Get();
        var led = GetField<Renderer>(comp, "LED", isPublic: true).Get();

        yield return WaitForSolve;

        if (colors.Length == 9)
            led.material = colors[8];
        addQuestion(module, Question.HiddenColorsLED, correctAnswers: new[] { ledcolors[ledcolor] });
    }

    private IEnumerator<YieldInstruction> ProcessHiddenValue(ModuleData module)
    {
        var comp = GetComponent(module, "hiddenValue");
        var numbers = GetListField<int>(comp, "numbers")
            .Get(minLength: 4, maxLength: 6, validator: v => v is < 0 or > 9 ? "Out of range [0, 9]" : null)
            .ToArray(); // Make a copy so the module can't modify it
        var colors = GetListField<char>(comp, "numberColors")
            .Get(expectedLength: numbers.Length, validator: v => "RGWYMCP".Contains(v) ? null : "Not in “RGWYMCP”")
            .Select(c => "RGWYMCP".IndexOf(c))
            .ToArray();

        yield return WaitForSolve;

        var format = translateString(Question.HiddenValueDisplay, "{0} {1}");
        var colorNames = new[] { "Red", "Green", "White", "Yellow", "Magenta", "Cyan", "Purple" }
            .Select(s => translateString(Question.HiddenValueDisplay, s))
            .ToArray();
        var all = from i in Enumerable.Range(0, 10) from c in colorNames select string.Format(format, c, i);
        var correct = numbers.Select((n, i) => string.Format(format, colorNames[colors[i]], n)).ToArray();
        addQuestion(module, Question.HiddenValueDisplay, correctAnswers: correct, allAnswers: all.ToArray());
    }

    private IEnumerator<YieldInstruction> ProcessHighScore(ModuleData module)
    {
        var comp = GetComponent(module, "HighScore");
        yield return WaitForSolve;

        var highScores = GetField<Array>(comp, "highScores").Get();
        var fldScore = GetIntField(highScores.GetValue(0), "score", isPublic: true);

        var playerPosition = GetIntField(comp, "entryNum").Get();
        var playerScore = fldScore.GetFrom(highScores.GetValue(playerPosition));

        var stringPos = playerPosition switch
        {
            0 => "1st",
            1 => "2nd",
            2 => "3rd",
            3 => "4th",
            _ => "5th"
        };

        addQuestions(module,
            makeQuestion(Question.HighScorePosition, module, correctAnswers: new[] { stringPos }),
            makeQuestion(Question.HighScoreScore, module, correctAnswers: new[] { "" + playerScore }));
    }

    private IEnumerator<YieldInstruction> ProcessHillCycle(ModuleData module)
    {
        return processSpeakingEvilCycle(module, "HillCycleScript", Question.HillCycleDialDirections, Question.HillCycleDialLabels);
    }

    private IEnumerator<YieldInstruction> ProcessHinges(ModuleData module)
    {
        var comp = GetComponent(module, "Hinges");
        var initialHingesStatus = GetArrayField<int>(comp, "hingeStatus").Get(expectedLength: 8, validator: i => i != 0 && i != 1 ? "expected value 0 or 1" : null).ToArray();

        yield return WaitForSolve;

        var qs = new List<QandA>();
        var presentHinges = new List<Sprite>();
        var absentHinges = new List<Sprite>();
        for (int pos = 0; pos < 8; pos++)
            (initialHingesStatus[pos] == 1 ? presentHinges : absentHinges).Add(HingesSprites[pos]);

        // There are eight hinges in total, so at least one question will generate.
        if (presentHinges.Count <= 4)
            qs.Add(makeQuestion(Question.HingesInitialHinges, module, formatArgs: new[] { "present on" }, correctAnswers: presentHinges.ToArray()));
        if (absentHinges.Count <= 4)
            qs.Add(makeQuestion(Question.HingesInitialHinges, module, formatArgs: new[] { "absent from" }, correctAnswers: absentHinges.ToArray()));
        addQuestions(module, qs);
    }

    private IEnumerator<YieldInstruction> ProcessHogwarts(ModuleData module)
    {
        var comp = GetComponent(module, "HogwartsModule");
        var fldModuleNames = GetField<IDictionary>(comp, "_moduleNames");
        yield return WaitForSolve;

        var dic = fldModuleNames.Get();
        if (dic.Count == 0)
        {
            Debug.Log($"[Souvenir #{_moduleId}] No question on Hogwarts because no module solves were awarded to it.");
            _legitimatelyNoQuestions.Add(module.Module);
            yield break;
        }

        // Rock-Paper-Scissors-Lizard-Spock needs to be broken up in the question because hyphens don't word-wrap.
        addQuestions(module,
            dic.Keys.Cast<object>().Where(house => dic[house] != null).SelectMany(house => Ut.NewArray(
                makeQuestion(Question.HogwartsHouse, module,
                    formatArgs: new[] { dic[house].ToString() == "Rock-Paper-Scissors-L.-Sp." ? "Rock-Paper- Scissors-L.-Sp." : dic[house].ToString() },
                    correctAnswers: new[] { house.ToString() }),
                makeQuestion(Question.HogwartsModule, module,
                    formatArgs: new[] { house.ToString() },
                    correctAnswers: new[] { dic[house].ToString() },
                    preferredWrongAnswers: Bomb.GetSolvableModuleNames().ToArray()))));
    }

    private IEnumerator<YieldInstruction> ProcessHoldUps(ModuleData module)
    {
        var comp = GetComponent(module, "HoldUpsScript");

        var stageNumber = GetField<int>(comp, "StageNr");
        var isItFiveStages = GetField<bool>(comp, "FiveDowns");

        var shadows = new List<string>();
        var holdUps = Enumerable.Range(1, 4).Select(btn => GetField<KMSelectable>(comp, $"Move{btn}Button", isPublic: true).Get()).ToArray();
        var prevInteracts = holdUps.Select(btn => btn.OnInteract).ToArray();

        foreach (var btn in Enumerable.Range(0, holdUps.Length))
        {
            holdUps[btn].OnInteract = delegate
            {
                if (shadows.Count < stageNumber.Get())
                    shadows.Add(GetField<TextMesh>(comp, "ShadowName", isPublic: true).Get().text);
                return prevInteracts[btn]();
            };
        }

        yield return WaitForSolve;

        addQuestions(module, Enumerable.Range(0, isItFiveStages.Get() ? 5 : 3).Select(stage => makeQuestion(Question.HoldUpsShadows, module, formatArgs: new[] { Ordinal(stage + 1) }, correctAnswers: new[] { shadows[stage] })));
    }

    private IEnumerator<YieldInstruction> ProcessHomophones(ModuleData module)
    {
        var comp = GetComponent(module, "HomophonesScript");
        yield return WaitForSolve;

        var selectedWords = GetArrayField<string>(comp, "selectedWords", true).Get(expectedLength: 4);

        // Set up a trick to prevent the answer from being obvious
        var allIWords = GetArrayField<string>(comp, "iWords").Get(expectedLength: 10);
        var allLWords = GetArrayField<string>(comp, "lWords").Get(expectedLength: 10);
        var allCWords = GetArrayField<string>(comp, "cWords").Get(expectedLength: 10);
        var allOneWords = GetArrayField<string>(comp, "oneWords").Get(expectedLength: 10);

        var possibleQuestions = new List<QandA>();

        for (int i = 0; i < selectedWords.Length; i++)
        {
            string thisWord = selectedWords[i];
            if (allCWords.Contains(thisWord))
                possibleQuestions.Add(makeQuestion(Question.HomophonesDisplayedPhrases, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { thisWord }, preferredWrongAnswers: selectedWords.Union(allCWords).ToArray()));
            else if (allLWords.Contains(thisWord))
                possibleQuestions.Add(makeQuestion(Question.HomophonesDisplayedPhrases, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { thisWord }, preferredWrongAnswers: selectedWords.Union(allLWords).ToArray()));
            else if (allIWords.Contains(thisWord))
                possibleQuestions.Add(makeQuestion(Question.HomophonesDisplayedPhrases, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { thisWord }, preferredWrongAnswers: selectedWords.Union(allIWords).ToArray()));
            else if (allOneWords.Contains(thisWord))
                possibleQuestions.Add(makeQuestion(Question.HomophonesDisplayedPhrases, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { thisWord }, preferredWrongAnswers: selectedWords.Union(allOneWords).ToArray()));
            else
                throw new AbandonModuleException($"The given phrase “{thisWord}” is not one of the possible words that can be found in Homophones.");
        }

        addQuestions(module, possibleQuestions);
    }

    private IEnumerator<YieldInstruction> ProcessHorribleMemory(ModuleData module)
    {
        var comp = GetComponent(module, "cruelMemoryScript");
        yield return WaitForSolve;

        var pos = GetListField<int>(comp, "correctStagePositions", isPublic: true).Get(expectedLength: 5);
        var lbl = GetListField<int>(comp, "correctStageLabels", isPublic: true).Get(expectedLength: 5);
        var colors = GetListField<string>(comp, "correctStageColours", isPublic: true).Get(expectedLength: 5);

        addQuestions(module,
            makeQuestion(Question.HorribleMemoryPositions, module, formatArgs: new[] { "first" }, correctAnswers: new[] { pos[0].ToString() }),
            makeQuestion(Question.HorribleMemoryPositions, module, formatArgs: new[] { "second" }, correctAnswers: new[] { pos[1].ToString() }),
            makeQuestion(Question.HorribleMemoryPositions, module, formatArgs: new[] { "third" }, correctAnswers: new[] { pos[2].ToString() }),
            makeQuestion(Question.HorribleMemoryPositions, module, formatArgs: new[] { "fourth" }, correctAnswers: new[] { pos[3].ToString() }),
            makeQuestion(Question.HorribleMemoryLabels, module, formatArgs: new[] { "first" }, correctAnswers: new[] { lbl[0].ToString() }),
            makeQuestion(Question.HorribleMemoryLabels, module, formatArgs: new[] { "second" }, correctAnswers: new[] { lbl[1].ToString() }),
            makeQuestion(Question.HorribleMemoryLabels, module, formatArgs: new[] { "third" }, correctAnswers: new[] { lbl[2].ToString() }),
            makeQuestion(Question.HorribleMemoryLabels, module, formatArgs: new[] { "fourth" }, correctAnswers: new[] { lbl[3].ToString() }),
            makeQuestion(Question.HorribleMemoryColors, module, formatArgs: new[] { "first" }, correctAnswers: new[] { colors[0] }),
            makeQuestion(Question.HorribleMemoryColors, module, formatArgs: new[] { "second" }, correctAnswers: new[] { colors[1] }),
            makeQuestion(Question.HorribleMemoryColors, module, formatArgs: new[] { "third" }, correctAnswers: new[] { colors[2] }),
            makeQuestion(Question.HorribleMemoryColors, module, formatArgs: new[] { "fourth" }, correctAnswers: new[] { colors[3] }));
    }

    private IEnumerator<YieldInstruction> ProcessHumanResources(ModuleData module)
    {
        var comp = GetComponent(module, "HumanResourcesModule");
        var people = GetStaticField<Array>(comp.GetType(), "_people").Get(ar => ar.Length != 16 ? "expected length 16" : null);
        var personToFire = GetIntField(comp, "_personToFire").Get(0, 15);
        var personToHire = GetIntField(comp, "_personToHire").Get(0, 15);

        var person = people.GetValue(0);
        var fldName = GetField<string>(person, "Name", isPublic: true);
        var fldDesc = GetField<string>(person, "Descriptor", isPublic: true);

        yield return WaitForSolve;

        var descs = GetArrayField<int>(comp, "_availableDescs").Get(expectedLength: 5);
        addQuestions(module,
            makeQuestion(Question.HumanResourcesDescriptors, module, formatArgs: new[] { "red" }, correctAnswers: descs.Take(3).Select(ix => fldDesc.GetFrom(people.GetValue(ix))).ToArray()),
            makeQuestion(Question.HumanResourcesDescriptors, module, formatArgs: new[] { "green" }, correctAnswers: descs.Skip(3).Select(ix => fldDesc.GetFrom(people.GetValue(ix))).ToArray()),
            makeQuestion(Question.HumanResourcesHiredFired, module, formatArgs: new[] { "fired" }, correctAnswers: new[] { fldName.GetFrom(people.GetValue(personToFire)) }),
            makeQuestion(Question.HumanResourcesHiredFired, module, formatArgs: new[] { "hired" }, correctAnswers: new[] { fldName.GetFrom(people.GetValue(personToHire)) }));
    }

    private IEnumerator<YieldInstruction> ProcessHunting(ModuleData module)
    {
        var comp = GetComponent(module, "hunting");
        var fldStage = GetIntField(comp, "stage");
        var fldReverseClues = GetField<bool>(comp, "reverseClues");

        while (!_isActivated)
            yield return new WaitForSeconds(.1f);

        var hasRowFirst = new bool[4];
        while (module.Unsolved)
        {
            hasRowFirst[fldStage.Get() - 1] = fldReverseClues.Get();
            yield return new WaitForSeconds(.1f);
        }

        var qs = new List<QandA>();
        foreach (var row in new[] { false, true })
            foreach (var first in new[] { false, true })
                qs.Add(makeQuestion(Question.HuntingColumnsRows, module,
                    formatArgs: new[] { row ? "row" : "column", first ? "first" : "second" },
                    correctAnswers: new[] { Question.HuntingColumnsRows.GetAnswers()[(hasRowFirst[0] ^ row ^ first ? 1 : 0) | (hasRowFirst[1] ^ row ^ first ? 2 : 0) | (hasRowFirst[2] ^ row ^ first ? 4 : 0)] }));
        addQuestions(module, qs);
    }

    private IEnumerator<YieldInstruction> ProcessHypercube(ModuleData module)
    {
        return processHypercubeUltracube(module, "TheHypercubeModule", Question.HypercubeRotations);
    }

    private List<List<string>> _hyperForgetStages = new();
    private IEnumerator<YieldInstruction> ProcessHyperForget(ModuleData module)
    {
        var comp = GetComponent(module, "HyperForget");
        const string moduleId = "HyperForget";

        yield return null;

        if (module.IsSolved)
        {
            _hyperForgetStages.Add(new());
            legitimatelyNoQuestion(module, "No question for HyperForget because there were no stages.");
            yield break;
        }

        var rots = GetListField<string>(comp, "rotationList").Get(minLength: 1);
        _hyperForgetStages.Add(rots);

        yield return null;

        if (_hyperForgetStages.Select(s => s.Count).Distinct().Count() is not 1)
            throw new AbandonModuleException($"Expected consistent stage counts among HyperForget modules, got {_hyperForgetStages.Select(s => s.Count).JoinString(", ")}");

        while (!_noUnignoredModulesLeft)
            yield return new WaitForSeconds(.1f);

        if (_hyperForgetStages.Count != _moduleCounts[moduleId])
            throw new AbandonModuleException("The number of handlers did not match the number of HyperForget modules.");

        var currentStage = GetField<int>(comp, "currentStage").Get();
        if (currentStage < 1)
        {
            legitimatelyNoQuestion(module, "No question for HyperForget because not enough stages were shown.");
            yield break;
        }

        if (_moduleCounts[moduleId] == 1)
        {
            // Only one HyperForget: No need for the disambiguation phrase
            addQuestions(module, rots.Take(currentStage).Select((rot, ix) => makeQuestion(Question.HyperForgetRotations, moduleId, 1, formatArgs: new[] { Ordinal(ix + 1) }, correctAnswers: new[] { rot })));
            yield break;
        }

        var uniqueStages = Enumerable.Range(1, currentStage).Where(stage => _hyperForgetStages.Count(display => display[stage - 1] == rots[stage - 1]) == 1).Take(2).ToArray();
        if (uniqueStages.Length == 0 || currentStage == 1)
        {
            var id = GetField<int>(comp, "moduleId").Get();
            legitimatelyNoQuestion(module, $"No question for HyperForget #{id} because there are not enough stages at which this one had a unique rotation.");
            yield break;
        }

        var qs = new List<QandA>();
        for (int stage = 0; stage < currentStage; stage++)
        {
            var uniqueStage = uniqueStages.FirstOrDefault(s => s - 1 != stage);
            if (uniqueStage != 0)
            {
                qs.Add(makeQuestion(Question.HyperForgetRotations, moduleId, 0,
                    formattedModuleName: string.Format(translateString(Question.HyperForgetRotations, "the HyperForget whose rotation in the {1} stage was {0}"), rots[uniqueStage - 1], Ordinal(uniqueStage)),
                    formatArgs: new[] { Ordinal(stage + 1) }, correctAnswers: new[] { rots[stage] }));
            }
        }
        addQuestions(module, qs);
    }

    private IEnumerator<YieldInstruction> ProcessHyperlink(ModuleData module)
    {
        var comp = GetComponent(module, "hyperlinkScript");
        yield return WaitForSolve;

        var moduleNamesType = comp.GetType().Assembly.GetType("IDList") ?? throw new AbandonModuleException("I cannot find the IDList type.");
        var moduleNames = GetStaticField<string[]>(moduleNamesType, "phrases", isPublic: true).Get(validator: ar => ar.Length % 2 != 0 ? "expected even number of items" : null);
        var hyperlink = GetField<string>(comp, "selectedString").Get();
        var anchor = GetIntField(comp, "anchor").Get();

        var questions = new List<QandA>();
        for (var i = 0; i < 11; i++)
            questions.Add(makeQuestion(Question.HyperlinkCharacters, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { hyperlink[i].ToString() }));
        questions.Add(makeQuestion(Question.HyperlinkAnswer, module, correctAnswers: new[] { moduleNames[anchor + 1].Replace("'", "’") }));

        addQuestions(module, questions);
    }
}
