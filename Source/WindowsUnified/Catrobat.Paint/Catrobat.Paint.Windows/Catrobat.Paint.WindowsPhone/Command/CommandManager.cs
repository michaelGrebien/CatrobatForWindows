﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Catrobat.Paint.Phone.Tool;
using Catrobat.Paint.Phone.Ui;
using Windows.UI.Xaml.Media.Imaging;
using Catrobat.Paint.WindowsPhone.Tool;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Catrobat.Paint.Phone.Command
{
    //TODO: what has to be synchronized?
    public class CommandManager
    {
        private static CommandManager _instance;
        private readonly LinkedList<CommandBase> _undoCommands;
        private readonly LinkedList<CommandBase> _redoCommands;
        //private readonly int MAX_COMMANDS = 12;
        private readonly Dictionary<CommandBase, WriteableBitmap> _commandBitmapDict;
        // TODO: private readonly BackgroundWorker _backgroundWorkerUndo = new BackgroundWorker();
        // TODO: private readonly BackgroundWorker _backgroundWorkerRedo = new BackgroundWorker();


        private CommandManager()
        {
            _undoCommands = new LinkedList<CommandBase>();
            _redoCommands = new LinkedList<CommandBase>();
            _commandBitmapDict = new Dictionary<CommandBase, WriteableBitmap>();

            /* TODO: 
            _backgroundWorkerUndo.WorkerReportsProgress = false;
            _backgroundWorkerUndo.WorkerSupportsCancellation = false;
            _backgroundWorkerUndo.DoWork += BackgroundWorkerUndoDoWork;
            _backgroundWorkerUndo.RunWorkerCompleted += BackgroundWorkerUndoRunWorkerCompleted;

            _backgroundWorkerRedo.WorkerReportsProgress = false;
            _backgroundWorkerRedo.WorkerSupportsCancellation = false;
            _backgroundWorkerRedo.DoWork += BackgroundWorkerRedoDoWork;
            _backgroundWorkerRedo.RunWorkerCompleted += BackgroundWorkerUndoRunWorkerCompleted;
            */
        }

        public static CommandManager GetInstance()
        {
            return _instance ?? (_instance = new CommandManager());
        }


        public bool HasCommands()
        {
            System.Diagnostics.Debug.WriteLine("UNDOCOMMANDS: " + _undoCommands.Count);
            return _undoCommands.Count > 0;
        }

        public bool HasNext()
        {
            System.Diagnostics.Debug.WriteLine("REDOCOMMANDS: " + _redoCommands.Count);
            return _redoCommands.Count > 0;
        }

        public void UnDo()
        {
            System.Diagnostics.Debug.WriteLine("--UNDO--");
            if (HasCommands())
            {
                /* TODO: if (_backgroundWorkerUndo.IsBusy || _backgroundWorkerRedo.IsBusy)
                {
                    return;
                }
                */
                Spinner.StartSpinning();

                var command = _undoCommands.Last.Value;
                _undoCommands.RemoveLast();

                /*CK: if (command is BrushCommand)
                {
                    var commandBrushStart = _undoCommands.Last.Value;
                    _undoCommands.RemoveLast();
                    _redoCommands.AddLast(commandBrushStart);
                }*/

                if (!HasCommands())
                {
                    UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.DisableUndo);
                }
                _redoCommands.AddLast(command);
                UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.EnableRedo);


                // TODO: _backgroundWorkerUndo.RunWorkerAsync(); //
                DoUnDo();
            }
        }

        private void DoUnDo()
        {
            //await Task.Run(delegate()
            //{
                ReDrawAll();
            //});
            Spinner.StopSpinning();
        }

        private void ReDrawAll()
        {
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Clear();
            PocketPaintApplication.GetInstance().PaintingAreaCanvasUnderlaying.Children.Clear();
                      
            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Background = null;
            PocketPaintApplication.GetInstance().PaintingAreaLayoutRoot.InvalidateMeasure();
            PocketPaintApplication.GetInstance().SaveAsWriteableBitmapToRam();
            PocketPaintApplication.GetInstance().SetBitmapAsPaintingAreaCanvasBackground();
            
            foreach (var command in _undoCommands)
            {
                command.ReDo();
            }
        }

        public void ReDo()
        {
            System.Diagnostics.Debug.WriteLine("--REDO--");

            if (HasNext())
            {
                /* TODO: if (_backgroundWorkerRedo.IsBusy || _backgroundWorkerUndo.IsBusy)
                {
                    return;
                }*/

                Spinner.StartSpinning();

                var command = _redoCommands.Last.Value;
                _redoCommands.RemoveLast();

                var redoCommandsList = new List<CommandBase>();
                /*if (command is BrushCommand)
                {
                    var commandBrushEnd = _redoCommands.Last.Value;
                    _redoCommands.RemoveLast();
                    redoCommandsList.Add(commandBrushEnd);
                }*/

                if (!HasNext())
                {
                    UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.DisableRedo);
                }
                _undoCommands.AddLast(command);
                UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.EnableUndo);

                redoCommandsList.Add(command);
                // TODO: _backgroundWorkerRedo.RunWorkerAsync(command);
                DoReDo(redoCommandsList);
            }
        }

        private void DoReDo(List<CommandBase> redoCommandsList)
        {
            //await Task.Run(delegate()
            //{
                foreach (var currentCommand in redoCommandsList)
                {
                    currentCommand.ReDo();
                }
            //});
            Spinner.StopSpinning();
        }

        public int GetNumberOfCommands()
        {
            return _undoCommands.Count;
        }

        public void CommitCommand(CommandBase command)
        {
            if (_undoCommands.Count == 0)
            {
                PocketPaintApplication.GetInstance().SaveAsWriteableBitmapToRam();
                var w = PocketPaintApplication.GetInstance().Bitmap.PixelWidth;
                var h = PocketPaintApplication.GetInstance().Bitmap.PixelHeight;
                _commandBitmapDict.Add(command, new WriteableBitmap(w, h));
            }

            if (HasNext())
            {
                _redoCommands.Clear();
                UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.DisableRedo);
            }


            // need a Bitmap to undo/redo special command
            ;
            if (_undoCommands.Count != 0 && command is EraserCommand)
            {
                var c = _undoCommands.Last();
                if (c.ToolType != ToolType.Eraser)
                {
                    //PocketPaintApplication.GetInstance().SaveAsWriteableBitmapToRam();
                    // TODO: _commandBitmapDict.Add(command, new WriteableBitmap(PocketPaintApplication.GetInstance().Bitmap));
                    var w = PocketPaintApplication.GetInstance().Bitmap.PixelWidth;
                    var h = PocketPaintApplication.GetInstance().Bitmap.PixelHeight;
                    _commandBitmapDict.Add(command, new WriteableBitmap(w, h));
                }

            }

            _undoCommands.AddLast(command);
            UndoRedoActionbarManager.GetInstance().Update(UndoRedoActionbarManager.UndoRedoButtonState.EnableUndo);
            PocketPaintApplication.GetInstance().UnsavedChangesMade = true;

        }


        // needs to be dispatched back to UIThread
        // started backgroundworkers to let UIThread show spinner etc. right before calculation starts
        // TODO: 

        /* TODO:
        private void BackgroundWorkerUndoDoWork(object sender, DoWorkEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(ReDrawAll);
        }
        private void BackgroundWorkerRedoDoWork(object sender, DoWorkEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var command = e.Argument as CommandBase;
                if (command != null)
                {
                    command.ReDo();
                }
            });
        }
        
        private void BackgroundWorkerUndoRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Spinner.StopSpinning();
        }*/
    }
}