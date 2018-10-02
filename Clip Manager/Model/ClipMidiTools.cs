using NAudio.Midi;

namespace Clip_Manager.Model
{
	public class ClipMidiTools
	{
		public static int IndexForNote(int note) {
			switch (note) {
				case 36: return 0;
				case 37: return 1;
				case 38: return 2;
				case 39: return 3;
				case 40: return 4;
				case 41: return 5;
				case 42: return 6;
				case 43: return 7;
			}

			return -1;
		}

		public static int NoteForIndex(int index) {
			switch (index) {
				case 0: return 36;
				case 1: return 37;
				case 2: return 38;
				case 3: return 39;
				case 4: return 40;
				case 5: return 41;
				case 6: return 42;
				case 7: return 43;
			}

			return 0;
		}

		public static bool IsCompatibleDeviceName(string name) {
			return name.Contains("LPD") ||
				name.Contains("MPC") ||
				name.Contains("Akai");
		}

		public static void SetIndexActivityIndicator(MidiOut device, int index, bool on) {
			var note = NoteForIndex(index);

			if (device == null || note <= 0) {
				return;
			}

			var evt = new NoteEvent(
				0, 1,
				on ? MidiCommandCode.NoteOn : MidiCommandCode.NoteOff,
				note, 1
			);

			device.Send(evt.GetAsShortMessage());
		}
	}
}
