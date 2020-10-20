using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public static class ResultPackDataBuilder
{
    public static ResultPackData Build(bool generated, bool autoplay, int keyCount, SongMetadata songMetadata, List<JudgmentRecord> judgeRecords, List<NotePoint> notes, int maxCombo, float percentage, float analysisTime)
    {
        ResultPackData packData = new ResultPackData();

        int perfectCount = 0;
        int greatCount = 0;
        int goodCount = 0;
        int missCount = 0;
        int goodHoldCount = 0;
        int badHoldCount = 0;
        int earlyCount = 0;
        int lateCount = 0;
        int score = 0;
        float maxEarly = 0;
        float maxLate = 0;
        float avgTime = 0;
        float sumOfDerivation = 0;

        foreach (JudgmentRecord r in judgeRecords)
        {
            switch (r.judgement)
            {
                case Judgement.PERFECT:
                    perfectCount++;
                    score += 1000;
                    avgTime += Mathf.Abs(r.offset);
                    break;
                case Judgement.GREAT:
                    greatCount++;
                    score += 500;
                    avgTime += Mathf.Abs(r.offset);
                    break;
                case Judgement.GOOD:
                    goodCount++;
                    score += 250;
                    avgTime += Mathf.Abs(r.offset);
                    break;
                case Judgement.GOOD_RELEASED:
                    goodHoldCount++;
                    score += 1000;
                    break;
                case Judgement.BAD_RELEASED:
                    badHoldCount++;
                    break;
                case Judgement.MISS:
                    avgTime += Mathf.Abs(r.offset);
                    goto case Judgement.UNPRESSED;
                case Judgement.UNPRESSED:
                    missCount++;
                    break;
            }

            if (r.offset > 0)
            {
                if (r.judgement != Judgement.UNPRESSED || r.judgement != Judgement.GOOD_RELEASED || r.judgement != Judgement.BAD_RELEASED)
                {
                    if (r.judgement != Judgement.PERFECT)
                    {
                        lateCount++;
                    }

                    if (r.offset > maxLate)
                    {
                        maxLate = r.offset;
                    }
                }
            }
            else
            {
                if (r.judgement != Judgement.UNPRESSED || r.judgement != Judgement.GOOD_RELEASED)
                {
                    if (r.judgement != Judgement.PERFECT)
                    {
                        earlyCount++;
                    }

                    if (r.offset < maxEarly)
                    {
                        maxEarly = r.offset;
                    }
                }
            }
        }

        avgTime /= (perfectCount + greatCount + goodCount + missCount);
        foreach (JudgmentRecord r in judgeRecords)
        {
            if (r.judgement == Judgement.PERFECT || r.judgement == Judgement.GREAT || r.judgement == Judgement.GOOD || r.judgement == Judgement.MISS)
            {
                sumOfDerivation += Mathf.Pow(Mathf.Abs(r.offset) - avgTime, 2);
            }
        }

        float sumOfDerivationAverage = sumOfDerivation / (perfectCount + greatCount + goodCount + missCount);
        packData.generated = generated;
        packData.perfectCount = perfectCount;
        packData.greatCount = greatCount;
        packData.goodCount = goodCount;
        packData.missCount = missCount;
        packData.goodHoldCount = goodHoldCount;
        packData.badHoldCount = badHoldCount;
        packData.accuracyPercentage = percentage;
        packData.score = score;
        packData.earlyCount = earlyCount;
        packData.lateCount = lateCount;
        packData.averageTime = avgTime;
        packData.stdDevTime = Mathf.Sqrt(sumOfDerivationAverage);
        packData.levelNotes = notes;
        packData.maxCombo = maxCombo;
        packData.metadata = songMetadata;
        packData.maxEarly = maxEarly;
        packData.maxLate = maxLate;
        packData.autoplay = autoplay;
        packData.judgeRecords = judgeRecords;
        packData.analysisTime = analysisTime;
        packData.keyCount = keyCount;
        packData.reportGeneratedTime = string.Format("{0:r}", DateTime.Now);
        return packData;
    }
}
