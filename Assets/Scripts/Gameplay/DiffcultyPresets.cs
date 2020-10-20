using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DiffcultyPresets
{

    static GenerationParam[] gParams = new GenerationParam[] {
        // easy
        new GenerationParam(
            0, //seed
            0.53f, // onset threshold
            2, // beat snapper divider
            0.002f, // beat snapping error threshold
            1, // multipleLaneChance
            3, // onBeatMultipleLaneChance
            1, // repeatedLaneTime
            new int[] { 100, 0, 0 }, // 4 lanes chance
            new int[] { 100, 0, 0, 0, 0 } // 6 lanes chance
            ),
        // medium
        new GenerationParam(
            0,
            1.00f,
            16,
            0.01f,
            5,
            5,
            1,
            new int[] { 100, 1, 0 },
            new int[] { 100, 5, 1, 0, 0 }
            ),
        // hard
        new GenerationParam(
            0,
            0.7f,
            32,
            0.01f,
            10,
            20,
            2,
            new int[] { 100, 2, 1 },
            new int[] { 100, 10, 5, 1, 0 }
            ),
        // insane
        new GenerationParam(
            0,
            0.7f,
            0,
            0.01f,
            15,
            30,
            4,
            new int[] { 100, 10, 2 },
            new int[] { 100, 50, 5, 1, 1 }
            )
    };
    public static GenerationParam GetPreset(int index)
    {
        return gParams[index];
    }
}
