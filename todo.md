# ShowControl - TODO List

## High Priority

### Feedback on button press
- [ ] Color change on slide/custom button press
- [ ] Change slide border color on press, listen to osc acknowledgement and change last pressed slide button accordingly to what was received

### Edit mode
- [ ] Changes window, pressing button brings up a editing modal
- [ ] Each chapter gets a button to add more slides
- [ ] Text labels (event name/chapter names) become editable text elements
- [ ] At bottom you can add more chapters
- [ ] Buttons next to chapter title to move it up/down in the hierarchy
- [ ] Save button to write changes to json file (brings up save menu)

### Top bar updates
- [ ] On the right: logo
- [ ] Left to that "ShowControl" + version number
- [ ] Settings button that opens up a program settings modal
- [ ] Move buttons/row to settings modal
- [ ] Add more other settings
- [ ] Write settings to a config.json file that's read on startup (if not there, loads up some defaults)

### New buttons
- [ ] Add "Next" button to left of "Take", checks last slide played and cues the next one in the json (jumps to next chapter if current slide is last in its chapter)
- [ ] Add (toggle) button "Skip animations" to left of "Next" button, tells ventuz to skip playing in/out animations

## Later additions
- [ ] Ventuz integration directly
- [ ] Communicate over remoting/vms instead of osc
- [ ] Ventuz tab in settings modal for connecting to machines, loading project, loading scene