import argparse
import multiprocessing
import os
import time

from madmom.features.beats import RNNBeatProcessor, MultiModelSelectionProcessor, DBNBeatTrackingProcessor
from madmom.features.notes import RNNPianoNoteProcessor, NotePeakPickingProcessor
from madmom.features.onsets import CNNOnsetProcessor, OnsetPeakPickingProcessor
from madmom.features.tempo import TempoEstimationProcessor
from madmom.io.audio import load_ffmpeg_file
from madmom.audio import SignalProcessor
from madmom.processors import SequentialProcessor, ParallelProcessor, IOProcessor

if __name__ == '__main__':
    multiprocessing.freeze_support()
    parser = argparse.ArgumentParser()

    parser.add_argument('filename')
    parser.add_argument('-ot', '--othreshold', type=float, default=1.35)
    parser.add_argument('-pt', '--pthreshold', type=float, default=0.35)
    parser.add_argument('-o', '--onsets-only', action='store_true')
    parser.add_argument('-b', '--beats-only', action='store_true')

    args = parser.parse_args()

    selector = MultiModelSelectionProcessor(None)
    rnn_beat_processor = RNNBeatProcessor(post_processor=selector)

    fps = 100
    pre_max = 1. / fps
    post_max = 1. / fps

    onset_peak_processor = OnsetPeakPickingProcessor(threshold=args.othreshold, smooth=0.05, fps=fps, pre_max=pre_max,
                                                     post_max=post_max)
    cnn_processor = CNNOnsetProcessor(fps=fps, pre_max=pre_max, post_max=post_max)
    beat_processor = DBNBeatTrackingProcessor(min_bpm=80, max_bpm=210, fps=100)
    SignalProcessor.add_arguments(parser, norm=False, gain=0)
    note_processor = RNNPianoNoteProcessor()
    peak_picking = NotePeakPickingProcessor(threshold=args.pthreshold, smooth=0.05, combine=0.03, fps=fps, pre_max=pre_max, post_max=post_max, delay=0)

    seq1 = IOProcessor([cnn_processor, onset_peak_processor])
    seq2 = SequentialProcessor([rnn_beat_processor, beat_processor])
    seq3 = SequentialProcessor([note_processor, peak_picking])
    audiofile, sample_rate = load_ffmpeg_file(args.filename, cmd_decode='./ffmpeg.exe', cmd_probe='./ffprobe.exe', sample_rate=44100)

    start_time = time.time()
    global onsets_result
    onsets_string = ''
    beats_string = ''

    cpu_count = 1
    if os.cpu_count() > 3:
        cpu_count = 4
    elif os.cpu_count() > 1:
        cpu_count = 2

    if args.beats_only:
        beats_result = seq2.process(audiofile)
        beats_string = ','.join(['%.2f' % num for num in beats_result])
        beats_string = 'b:' + beats_string

    elif args.onsets_only:
        parallels_processors = ParallelProcessor([seq1, seq3], cpu_count)
        onsets_result = parallels_processors.process(audiofile)
        # filter onsets
        onsets_string = 'o:'
        for onset in onsets_result[0]:
            onsets_string += '{:.2f}'.format(onset) + ';'
            for note in onsets_result[1]:
                if abs(note[0] - onset) < 0.02:
                    onsets_string += '{:.0f}'.format(note[1]) + '+'
            onsets_string += ','
        print('c:' + str(len(onsets_result[0])) + '\n')

    else:
        parallels_processors = ParallelProcessor([seq1, seq2, seq3], cpu_count)
        onsets_beats_result = parallels_processors.process(audiofile)
        # filter onsets
        onsets_string = 'o:'
        for onset in onsets_beats_result[0]:
            onsets_string += '{:.2f}'.format(onset) + ';'
            for note in onsets_beats_result[2]:
                if abs(note[0] - onset) < 0.02:
                    onsets_string += '{:.0f}'.format(note[1]) + '+'
            onsets_string += ','
        beats_string = ','.join(['%.2f' % num for num in onsets_beats_result[1]])
        beats_string = 'b:' + beats_string

    output_text = onsets_string + '\n' + beats_string + '\n' + "t:%s" % (time.time() - start_time)
    print(output_text, flush=True)