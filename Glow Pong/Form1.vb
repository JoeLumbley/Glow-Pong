'Glow Pong

'Ping pong game with a glow effect.
'This app is resizable, pausable and has a computer player.
'Supports keyboard, mouse and Xbox controllers including the vibration effect (rumble).
'It was written in 2023 and is compatible with Windows 10 and 11.

'MIT License
'Copyright(c) 2023 Joseph Lumbley

'Permission Is hereby granted, free Of charge, to any person obtaining a copy
'of this software And associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
'copies of the Software, And to permit persons to whom the Software Is
'furnished to do so, subject to the following conditions:

'The above copyright notice And this permission notice shall be included In all
'copies Or substantial portions of the Software.

'THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
'IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
'LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
'OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
'SOFTWARE.

'Monica is our an AI assistant.
'https://monica.im/

'I'm making a video to explain the code on my YouTube channel.
'https://www.youtube.com/@codewithjoe6074

Imports System.Runtime.InteropServices
Imports System.Numerics

Public Class Form1

    Private Enum GameStateEnum
        StartScreen = 0
        Instructions = 1
        Serve = 2
        Playing = 3
        EndScreen = 4
        Pause = 5
    End Enum

    Private Enum ServeStateEnum
        LeftPaddle = 1
        RightPaddle = 2
    End Enum

    Private Enum WinStateEnum
        LeftPaddle = 1
        RightPaddle = 2
    End Enum

    Private Enum DirectionEnum
        UpLeft = 1
        Left = 2
        DownLeft = 3
        UpRight = 4
        Right = 5
        DownRight = 6
    End Enum

    'Ball Data *************************
    Private Ball As Rectangle
    Private BallPos As New Vector2
    Private Const BallVelocity As Double = 500
    Private BallDirection As DirectionEnum
    Private ReadOnly BallMidlineUpPen As New Pen(Color.Green, 7)
    Private ReadOnly BallMidlineDownPen As New Pen(Color.Red, 7)
    Private BallMiddle As Single = 0
    '***********************************

    'Left Paddle Data *****************
    Private LeftPaddle As Rectangle
    Private LeftPaddlePos As New Vector2
    Private Const LeftPaddleVelocity As Double = 500
    Private LeftPaddleScore As Integer
    Private LPadScoreLocation As Point
    Private ReadOnly LeftPaddleMidlinePen As New Pen(Color.Goldenrod, 7)
    Private LeftPaddleMiddle As Single = 0
    '***********************************

    'Right Paddle Data *****************
    Private RightPaddle As Rectangle
    Private RightPaddlePos As New Vector2
    Private Const RightPaddleVelocity As Double = 500
    Private RightPaddleScore As Integer
    Private RPadScoreLocation As Point
    '***********************************

    Private ReadOnly ScoreFont As New Font(FontFamily.GenericSansSerif, 75)
    Private ReadOnly AlineCenterMiddle As New StringFormat

    Private InstructStartLocation As Point
    Private ReadOnly InstructStartText As String = vbCrLf &
        "One player:  A      Two players:  B"

    'One Player Instructions Data *************************
    Private InstructOneLocation As Point
    Private Const InstructOneText As String = vbCrLf &
        "Start:  B" & vbCrLf & vbCrLf &
        "Computer plays left paddle." & vbCrLf &
        "Right paddle use ↑  ↓ to move." & vbCrLf &
        "First player to 10 points wins." & vbCrLf & vbCrLf &
        "Pause:  Start  P" & vbCrLf &
        "Resume:  A  P"
    '******************************************************

    'Two Player Instructions Data *************************
    Private InstructTwoLocation As Point
    Private Const InstructTwoText As String = vbCrLf &
        "Start:  A  " & vbCrLf & vbCrLf &
        "Left paddle use  W  S or  ↑  ↓  to move." & vbCrLf &
        "Right paddle use  ↑  ↓  to move." & vbCrLf &
        "First player to 10 points wins." & vbCrLf & vbCrLf &
        "Pause:  Start  P" & vbCrLf &
        "Resume:  A  P"
    '******************************************************
    Private ReadOnly InstructionsFont As New Font(FontFamily.GenericSansSerif, 15)
    Private ReadOnly AlineCenter As New StringFormat

    'Title Data *******************************************
    Private Const TitleText As String = "GLOW PONG"
    Private TitleLocation As New Point(ClientSize.Width \ 2, ClientSize.Height \ 2 - 125)
    Private ReadOnly TitleFont As New Font(FontFamily.GenericSansSerif, 48)
    '******************************************************

    'Keyboard Event Data **********************************
    Private SpaceBarDown As Boolean = False
    Private WKeyDown As Boolean = False
    Private SKeyDown As Boolean = False
    Private UpArrowKeyDown As Boolean = False
    Private DownArrowKeyDown As Boolean = False
    Private OneKeyDown As Boolean = False
    Private TwoKeyDown As Boolean = False
    Private PKeyDown As Boolean = False
    Private AKeyDown As Boolean = False
    Private BKeyDown As Boolean = False
    Private XKeyDown As Boolean = False
    '******************************************************

    'Mouse Event Data *************************************
    Private MouseWheelUp As Boolean = False
    Private MouseWheelDown As Boolean = False
    '******************************************************

    'State Data *******************************************
    Private GameState As GameStateEnum = GameStateEnum.StartScreen
    Private Serving As ServeStateEnum = ServeStateEnum.RightPaddle
    Private Winner As WinStateEnum
    Private NumberOfPlayers As Integer = 1
    '******************************************************

    'Counter Data *************************************
    Private FlashCount As Integer = 0
    Private EndScreenCounter As Integer = 0
    '******************************************************

    'Back Buffer Data *************************************
    Private Context As BufferedGraphicsContext
    Private Buffer As BufferedGraphics
    '******************************************************

    'Centerline Data *******************
    Private CenterlineTop As Point
    Private CenterlineBottom As Point
    Private ReadOnly CenterlinePen As New Pen(Color.LightYellow, 7)
    '***********************************

    'Wall Data ***************************************
    Private Const TopWall As Integer = 0
    Private BottomWall As Integer = ClientSize.Height
    '*************************************************

    Private DrawFlashingText As Boolean = True

    <DllImport("XInput1_4.dll")>
    Private Shared Function XInputGetState(dwUserIndex As Integer, ByRef pState As XINPUT_STATE) As Integer
    End Function

    'XInput1_4.dll seems to be the current version
    'XInput9_1_0.dll is maintained primarily for backward compatibility. 

    <StructLayout(LayoutKind.Explicit)>
    Public Structure XINPUT_STATE
        <FieldOffset(0)>
        Public dwPacketNumber As UInteger 'Unsigned 32-bit (4-byte) integer range 0 through 4,294,967,295.
        <FieldOffset(4)>
        Public Gamepad As XINPUT_GAMEPAD
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure XINPUT_GAMEPAD
        Public wButtons As UShort 'Unsigned 16-bit (2-byte) integer range 0 through 65,535.
        Public bLeftTrigger As Byte 'Unsigned 8-bit (1-byte) integer range 0 through 255.
        Public bRightTrigger As Byte
        Public sThumbLX As Short 'Signed 16-bit (2-byte) integer range -32,768 through 32,767.
        Public sThumbLY As Short
        Public sThumbRX As Short
        Public sThumbRY As Short
    End Structure

    <DllImport("XInput1_4.dll")>
    Private Shared Function XInputSetState(playerIndex As Integer, ByRef vibration As XINPUT_VIBRATION) As Integer
    End Function

    Public Structure XINPUT_VIBRATION
        Public wLeftMotorSpeed As UShort
        Public wRightMotorSpeed As UShort
    End Structure

    Private AControllerID As Integer = -1
    Private AControllerDown As Boolean = False
    Private AControllerUp As Boolean = False
    Private AControllerLeft As Boolean = False
    Private AControllerRight As Boolean = False
    Private AControllerA As Boolean = False
    Private AControllerB As Boolean = False
    Private AControllerStart As Boolean = False
    Private AControllerX As Boolean = False
    Private AControllerTsUp As Boolean = False
    Private AControllerTsDown As Boolean = False

    Private BControllerID As Integer = -1
    Private BControllerDown As Boolean = False
    Private BControllerUp As Boolean = False
    Private BControllerLeft As Boolean = False
    Private BControllerRight As Boolean = False
    Private BControllerA As Boolean = False
    Private BControllerB As Boolean = False
    Private BControllerStart As Boolean = False
    Private BControllerX As Boolean = False
    Private BControllerTsUp As Boolean = False
    Private BControllerTsDown As Boolean = False

    'The start of the thumbstick neutral zone.
    Private Const NeutralStart As Short = -16256 'Signed 16-bit (2-byte) integer range -32,768 through 32,767.

    'The end of the thumbstick neutral zone.
    Private Const NeutralEnd As Short = 16256

    'Private ControllerData As JOYINFOEX
    Private ControllerPosition As XINPUT_STATE

    Private ControllerNumber As Long = 0

    Private Vibration As XINPUT_VIBRATION
    '***************************************************************************************************
    Private ClientCenter As New Point(ClientSize.Width \ 2, ClientSize.Height \ 2)

    Private CurrentFrame As DateTime = Now 'Initialize current frame to current time.

    Private LastFrame As DateTime = CurrentFrame 'Initialize last frame time to current time.

    Private DeltaTime As TimeSpan = CurrentFrame - LastFrame 'Initialize delta time to zero.

    Private RightPaddleHit As Boolean = False
    Private RightPaddleHitTimer As Integer = 0

    Private LeftPaddleHit As Boolean = False
    Private LeftPaddleHitTimer As Integer = 0

    Private Orchid0Pen As New Pen(Color.FromArgb(40, Color.DeepPink), 20) 'Bottom
    Private Orchid1Pen As New Pen(Color.FromArgb(50, Color.DeepPink), 17)
    Private Orchid2Pen As New Pen(Color.FromArgb(64, Color.DeepPink), 13)
    Private Orchid3Pen As New Pen(Color.FromArgb(150, Color.DeepPink), 8)
    Private Orchid4Pen As New Pen(Color.FromArgb(255, Color.Pink), 5) 'Top

    Private SkyBlue0Pen As New Pen(Color.FromArgb(40, Color.Blue), 20) 'Bottom
    Private SkyBlue1Pen As New Pen(Color.FromArgb(50, Color.Blue), 17)
    Private SkyBlue2Pen As New Pen(Color.FromArgb(64, Color.Blue), 13)
    Private SkyBlue3Pen As New Pen(Color.FromArgb(128, Color.Blue), 8)
    Private SkyBlue4Pen As New Pen(Color.FromArgb(255, Color.LightBlue), 5) 'Top

    Private DimmerBrush As New SolidBrush(Color.FromArgb(200, Color.Black))

    Private InstructPauseLocation As Point

    Private LeftScore0Brush As New SolidBrush(Color.FromArgb(20, Color.DeepPink)) 'Bottom
    Private LeftScore1Brush As New SolidBrush(Color.FromArgb(40, Color.DeepPink))
    Private LeftScore2Brush As New SolidBrush(Color.FromArgb(40, Color.DeepPink))
    Private LeftScore3Brush As New SolidBrush(Color.FromArgb(100, Color.DeepPink))
    Private LeftScore4Brush As New SolidBrush(Color.FromArgb(255, Color.Pink)) 'Top

    Private LeftScore0Font As New Font(FontFamily.GenericSansSerif, 75 + 16) 'Bottom
    Private LeftScore1Font As New Font(FontFamily.GenericSansSerif, 75 + 12)
    Private LeftScore2Font As New Font(FontFamily.GenericSansSerif, 75 + 8)
    Private LeftScore3Font As New Font(FontFamily.GenericSansSerif, 75 + 4)
    Private LeftScore4Font As New Font(FontFamily.GenericSansSerif, 75) 'Top

    Private GreenGlow0Brush As New SolidBrush(Color.FromArgb(40, Color.Green)) 'Bottom
    Private GreenGlow1Brush As New SolidBrush(Color.FromArgb(40, Color.Green))
    Private GreenGlow2Brush As New SolidBrush(Color.FromArgb(50, Color.Green))
    Private GreenGlow3Brush As New SolidBrush(Color.FromArgb(128, Color.Green))
    Private GreenGlow4Brush As New SolidBrush(Color.FromArgb(255, Color.LightGreen))

    Private TitleGlow0Font As New Font(FontFamily.GenericSansSerif, 48 + 16) 'Bottom
    Private TitleGlow1Font As New Font(FontFamily.GenericSansSerif, 48 + 12)
    Private TitleGlow2Font As New Font(FontFamily.GenericSansSerif, 48 + 8)
    Private TitleGlow3Font As New Font(FontFamily.GenericSansSerif, 48 + 4)
    Private TitleGlow4Font As New Font(FontFamily.GenericSansSerif, 48) 'Top

    Private RightScore0Font As New Font(FontFamily.GenericSansSerif, 75 + 16) 'Bottom
    Private RightScore1Font As New Font(FontFamily.GenericSansSerif, 75 + 12)
    Private RightScore2Font As New Font(FontFamily.GenericSansSerif, 75 + 8)
    Private RightScore3Font As New Font(FontFamily.GenericSansSerif, 75 + 4)
    Private RightScore4Font As New Font(FontFamily.GenericSansSerif, 75) 'Top

    Private CheckerboardBrush As New TextureBrush(My.Resources.checkerboard)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeGame()

    End Sub

    Private Sub InitializeGame()

        InitializeForm()

        LayoutInstructions()

        InitializePaddles()

        InitializeBall()

        InitializeGraphics()

        InitializeTimer()

    End Sub

    Private Sub InitializeTimer()

        Timer1.Interval = 15 '16ms = 1000 milliseconds \ 60 frames per second

        Timer1.Start()

    End Sub

    Private Sub InitializeGraphics()

        CenterlinePen.DashStyle = Drawing2D.DashStyle.Dash

        CheckerboardBrush.WrapMode = Drawing2D.WrapMode.Tile

        InitializeStringAlinement()

        InitializeGraphicsBuffer()

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        UpdateGame()

        Refresh() 'Calls OnPaint Event

    End Sub

    Private Sub UpdateGame()

        Select Case GameState

            Case GameStateEnum.EndScreen

                UpdateEndScreen()

            Case GameStateEnum.Playing

                UpdatePlaying()

            Case GameStateEnum.StartScreen

                UpdateStartScreen()

            Case GameStateEnum.Instructions

                UpdateInstructions()

            Case GameStateEnum.Serve

                UpdateServe()

            Case GameStateEnum.Pause

                UpdatePause()

        End Select

    End Sub

    Private Sub InitializeGraphicsBuffer()

        Context = BufferedGraphicsManager.Current

        Context.MaximumBuffer = Screen.PrimaryScreen.WorkingArea.Size

        Buffer = Context.Allocate(CreateGraphics(), ClientRectangle)

    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)

        DrawGame()

        'Show buffer on form.
        Buffer.Render(e.Graphics)

        'Release memory used by buffer.
        Buffer.Dispose()
        Buffer = Nothing

        'Create new buffer.
        Buffer = Context.Allocate(CreateGraphics(), ClientRectangle)

        'Use these settings when drawing to the backbuffer.
        With Buffer.Graphics

            'Bug fix don't change.
            .CompositingMode = Drawing2D.CompositingMode.SourceOver
            'To fix draw string error: "Parameters not valid."
            'Set the compositing mode to source over.

            .SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            .TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            .CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
            .InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            .PixelOffsetMode = Drawing2D.PixelOffsetMode.HighSpeed

        End With

    End Sub

    Private Sub UpdatePlaying()

        UpdateDeltaTime()

        UpdateControllerPosition()

        UpdatePaddles()

        UpdateBall()

        UpdateScore()

        CheckforEndGame()

        CheckForPause()

    End Sub

    Private Sub UpdateDeltaTime()

        CurrentFrame = Now ' get current time

        DeltaTime = CurrentFrame - LastFrame ' calculate delta time

        LastFrame = Now ' update last frame time

    End Sub

    Private Sub DrawGame()

        'Buffer.Graphics.Clear(Color.Black)

        Buffer.Graphics.FillRectangle(CheckerboardBrush, ClientRectangle)




        Select Case GameState
            Case GameStateEnum.EndScreen

                DrawEndScreen()

            Case GameStateEnum.StartScreen

                DrawStartScreen()

            Case GameStateEnum.Instructions

                DrawInstructions()

            Case GameStateEnum.Playing

                DrawPlaying()

            Case GameStateEnum.Serve

                DrawServe()

            Case GameStateEnum.Pause

                DrawPauseScreen()

        End Select

    End Sub

    Private Sub DrawPlaying()

        DrawCenterCourtLine()

        DrawLeftPaddle()

        DrawRightPaddle()

        DrawBall()

        DrawLeftPaddleScore()

        DrawRightPaddleScore()

        If NumberOfPlayers = 1 Then

            DrawComputerPlayerIdentifier()

        End If

    End Sub

    Private Sub UpdatePaddles()

        UpdateLeftPaddle()

        UpdateRightPaddle()

    End Sub

    Private Sub UpdateBall()

        MoveBall()

        CheckPaddleHits()

    End Sub

    Private Sub UpdateScore()

        'Did ball enter left goal zone?
        If BallPos.X < 0 Then
            'Yes, ball entered left goal zone.

            'Award point to right paddle.
            RightPaddleScore += 1

            'Change possession of ball to right paddle.
            Serving = ServeStateEnum.RightPaddle

            'Change game state to serve.
            GameState = GameStateEnum.Serve

            PlayScoreSound()

        End If

        'Did ball enter right goal zone?
        If BallPos.X + Ball.Width > ClientSize.Width Then
            'Yes, ball entered goal zone.

            'Award a point to left paddle.
            LeftPaddleScore += 1

            'Change possession of ball to left paddle.
            Serving = ServeStateEnum.LeftPaddle

            'Change game state to serve.
            GameState = GameStateEnum.Serve

            PlayScoreSound()

        End If

    End Sub

    Private Sub CheckforEndGame()

        'Did left paddle reach winning score?
        If LeftPaddleScore >= 10 Then
            'Yes, left paddle did reach winning score.

            'Set winner to left paddle.
            Winner = WinStateEnum.LeftPaddle

            'Reset the frame counter.
            FlashCount = 0

            'Change game state to end screen.
            GameState = GameStateEnum.EndScreen

            PlayWinningSound()

        End If

        'Did right paddle reach winning score?
        If RightPaddleScore >= 10 Then
            'Yes, right paddle did reach winning score.

            'Set winner to right paddle.
            Winner = WinStateEnum.RightPaddle

            'Reset frame counter.
            FlashCount = 0

            'Change game state to end screen.
            GameState = GameStateEnum.EndScreen

            PlayWinningSound()

        End If

    End Sub

    Private Sub CheckForPause()

        UpdateControllerPosition()

        If AControllerStart = True Or BControllerStart = True Then

            AControllerStart = False
            BControllerStart = False

            GameState = GameStateEnum.Pause

            PlayBounceSound()

        End If

        If PKeyDown = True Then

            PKeyDown = False

            GameState = GameStateEnum.Pause

        End If

    End Sub

    Private Sub UpdateLeftPaddle()

        If NumberOfPlayers = 1 Then

            UpdateLeftPaddleOnePlayer()

        Else

            UpdateLeftPaddleController()

            UpdateLeftPaddleTwoPlayer()

        End If

    End Sub

    Private Sub UpdateRightPaddle()

        UpdateRightPaddleController()

        UpdateRightPaddleKeyboard()

        UpdateRightPaddleMouse()

    End Sub
    Private Sub MoveBall()

        Select Case BallDirection

            Case DirectionEnum.UpLeft

                MoveBallUpLeft()

            Case DirectionEnum.Left

                MoveBallLeft()

            Case DirectionEnum.DownLeft

                MoveBallDownLeft()

            Case DirectionEnum.UpRight

                MoveBallUpRight()

            Case DirectionEnum.Right

                MoveBallRight()

            Case DirectionEnum.DownRight

                MoveBallDownRight()

        End Select

        BallMiddle = BallPos.Y + Ball.Height / 2.0F

    End Sub

    Private Sub CheckPaddleHits()

        CheckLeftPaddleHit()

        CheckRightPaddleHit()

    End Sub

    Private Sub UpdateLeftPaddleOnePlayer()
        'In one player mode the left paddle is played
        'by the following algorithm.

        'Is the ball above the paddle?
        If BallMiddle < LeftPaddleMiddle Then
            'Yes, the ball is above the paddle.

            'Move the paddle up.
            LeftPaddlePos.Y -= (LeftPaddleVelocity - 50.0F) * DeltaTime.TotalSeconds

            'Is the paddle above the playing field? 
            If LeftPaddlePos.Y < TopWall Then
                'Yes, the paddle is above the playing field.

                'Push the paddle back on to the playing field.
                LeftPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

            LeftPaddleMiddle = LeftPaddlePos.Y + LeftPaddle.Height / 2.0F

        End If

        'Is the ball below the paddle?
        If BallMiddle > LeftPaddleMiddle Then
            'Yes, the ball is below the paddle.

            'Move the paddle down.
            LeftPaddlePos.Y += (LeftPaddleVelocity - 50.0F) * DeltaTime.TotalSeconds

            'Is the paddle below the playing field?
            If LeftPaddlePos.Y + LeftPaddle.Height > BottomWall Then
                'Yes, the paddle is below the playing field.

                'Push the paddle back on to the playing field.
                LeftPaddlePos.Y = BottomWall - LeftPaddle.Height

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)


            LeftPaddleMiddle = LeftPaddlePos.Y + LeftPaddle.Height / 2.0F

        End If

    End Sub

    Private Sub UpdateLeftPaddleTwoPlayer()

        'Is the left player pressing the W key down?
        If WKeyDown = True Then
            'Yes, the left player is pressing the W key down.

            'Move left paddle up.
            LeftPaddlePos.Y -= LeftPaddleVelocity * DeltaTime.TotalSeconds


            'Is the left paddle above the playing field? 
            If LeftPaddlePos.Y < TopWall Then
                'Yes, the left paddle is above playing field.

                'Push the left paddle down and back into playing field.
                LeftPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        End If

        'Is the left player pressing the S key down?
        If SKeyDown = True Then
            'Yes, the left player is pressing the S key down.

            'Move left paddle down.
            LeftPaddlePos.Y += LeftPaddleVelocity * DeltaTime.TotalSeconds


            'Is the left paddle below the playing field?
            If LeftPaddlePos.Y + LeftPaddle.Height > BottomWall Then
                'Yes, the left paddle is below playing field.

                'Push the left paddle up and back into playing field.
                LeftPaddlePos.Y = BottomWall - LeftPaddle.Height

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        End If

    End Sub

    Private Sub UpdateRightPaddleKeyboard()

        'Is the right player pressing the up arrow key down?
        If UpArrowKeyDown = True Then
            'Yes, the right player is pressing the up arrow key down.

            'Move right paddle up.
            RightPaddlePos.Y -= RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle above the playing field?
            If RightPaddlePos.Y < TopWall Then
                'Yes, the right paddle is above playing field.

                'Push the right paddle down and back into playing field.
                RightPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        'Is the right paddle player pressing the down arrow key down?
        If DownArrowKeyDown = True Then
            'Yes, the right paddle player is pressing the down arrow key down.

            'Move right paddle down.
            RightPaddlePos.Y += RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle below the playing field?
            If RightPaddlePos.Y + RightPaddle.Height > BottomWall Then
                'Yes, the right paddle is below playing field.

                'Push the right paddle up and back into playing field.
                RightPaddlePos.Y = BottomWall - RightPaddle.Height

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

    End Sub

    Private Sub UpdateRightPaddleMouse()

        'Is the right paddle player rolling the mouse wheel up?
        If MouseWheelUp = True Then
            'Yes, the right paddle player is rolling the mouse wheel up.

            'Move right paddle up.
            RightPaddlePos.Y -= RightPaddleVelocity * 4 * DeltaTime.TotalSeconds

            'Is the right paddle above the playing field?
            If RightPaddlePos.Y < TopWall Then
                'Yes, the right paddle is above playing field.

                'Push the right paddle down and back into playing field.
                RightPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        'Is the right paddle player rolling the mouse wheel down?
        If MouseWheelDown = True Then
            'Yes, the right paddle player is rolling the mouse wheel down.

            'Move right paddle down.
            RightPaddlePos.Y += RightPaddleVelocity * 4 * DeltaTime.TotalSeconds

            'Is the right paddle below the playing field?
            If RightPaddlePos.Y + RightPaddle.Height > BottomWall Then
                'Yes, the right paddle is below playing field.

                'Push the right paddle up and back into playing field.
                RightPaddlePos.Y = BottomWall - RightPaddle.Height

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

    End Sub

    Private Sub UpdateRightPaddleController()

        If NumberOfPlayers = 2 Then

            UpdateRightPaddleControllerTwoPlayer()

        Else

            UpdateRightPaddleControllerOnePlayer()

        End If

    End Sub

    Private Sub UpdateRightPaddleControllerOnePlayer()

        If AControllerDown = True Then

            'Move right paddle down.
            RightPaddlePos.Y += RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle below the playing field?
            If RightPaddlePos.Y + RightPaddle.Height > BottomWall Then
                'Yes, the right paddle is below playing field.

                'Push the right paddle up and back into playing field.
                RightPaddlePos.Y = BottomWall - RightPaddle.Height

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        If AControllerUp = True Then

            'Move right paddle up.
            RightPaddlePos.Y -= RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle above the playing field?
            If RightPaddlePos.Y < TopWall Then
                'Yes, the right paddle is above playing field.

                'Push the right paddle down and back into playing field.
                RightPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        If AControllerTsDown = True Then

            'Move right paddle down.
            RightPaddlePos.Y += RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle below the playing field?
            If RightPaddlePos.Y + RightPaddle.Height > BottomWall Then
                'Yes, the right paddle is below playing field.

                'Push the right paddle up and back into playing field.
                RightPaddlePos.Y = BottomWall - RightPaddle.Height

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        If AControllerTsUp = True Then

            'Move right paddle up.
            RightPaddlePos.Y -= RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle above the playing field?
            If RightPaddlePos.Y < TopWall Then
                'Yes, the right paddle is above playing field.

                'Push the right paddle down and back into playing field.
                RightPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

    End Sub

    Private Sub UpdateRightPaddleControllerTwoPlayer()

        If BControllerDown = True Then

            'Move right paddle down.
            RightPaddlePos.Y += RightPaddleVelocity * DeltaTime.TotalSeconds

            'Is the right paddle below the playing field?
            If RightPaddlePos.Y + RightPaddle.Height > BottomWall Then
                'Yes, the right paddle is below playing field.

                'Push the right paddle up and back into playing field.
                RightPaddlePos.Y = BottomWall - RightPaddle.Height

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        If BControllerUp = True Then

            'Move right paddle up.
            RightPaddlePos.Y -= RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle above the playing field?
            If RightPaddlePos.Y < TopWall Then
                'Yes, the right paddle is above playing field.

                'Push the right paddle down and back into playing field.
                RightPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        If BControllerTsDown = True Then

            'Move right paddle down.
            RightPaddlePos.Y += RightPaddleVelocity * DeltaTime.TotalSeconds


            'Is the right paddle below the playing field?
            If RightPaddlePos.Y + RightPaddle.Height > BottomWall Then
                'Yes, the right paddle is below playing field.

                'Push the right paddle up and back into playing field.
                RightPaddlePos.Y = BottomWall - RightPaddle.Height

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

        If BControllerTsUp = True Then

            'Move right paddle up.
            'RightPaddle.Y -= deltaTime.TotalMilliseconds * RightPaddleVelocity

            RightPaddlePos.Y -= RightPaddleVelocity * DeltaTime.TotalSeconds



            'Is the right paddle above the playing field?
            If RightPaddlePos.Y < TopWall Then
                'Yes, the right paddle is above playing field.

                'Push the right paddle down and back into playing field.
                RightPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        End If

    End Sub

    Private Sub UpdateLeftPaddleController()

        If AControllerDown = True Then

            'Move left paddle down.
            LeftPaddlePos.Y += LeftPaddleVelocity * DeltaTime.TotalSeconds


            'Is the left paddle below the playing field?
            If LeftPaddlePos.Y + LeftPaddle.Height > BottomWall Then
                'Yes, the left paddle is below playing field.

                'Push the left paddle up and back into playing field.
                LeftPaddlePos.Y = BottomWall - LeftPaddle.Height

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        End If

        If AControllerUp = True Then

            'Move left paddle up.
            LeftPaddlePos.Y -= LeftPaddleVelocity * DeltaTime.TotalSeconds


            'Is the left paddle above the playing field? 
            If LeftPaddlePos.Y < TopWall Then
                'Yes, the left paddle is above playing field.

                'Push the left paddle down and back into playing field.
                LeftPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        End If

        If AControllerTsDown = True Then

            'Move left paddle down.
            LeftPaddlePos.Y += LeftPaddleVelocity * DeltaTime.TotalSeconds


            'Is the left paddle below the playing field?
            If LeftPaddlePos.Y + LeftPaddle.Height > BottomWall Then
                'Yes, the left paddle is below playing field.

                'Push the left paddle up and back into playing field.
                LeftPaddlePos.Y = BottomWall - LeftPaddle.Height

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        End If

        If AControllerTsUp = True Then

            'Move left paddle up.
            LeftPaddlePos.Y -= LeftPaddleVelocity * DeltaTime.TotalSeconds


            'Is the left paddle above the playing field? 
            If LeftPaddlePos.Y < TopWall Then
                'Yes, the left paddle is above playing field.

                'Push the left paddle down and back into playing field.
                LeftPaddlePos.Y = TopWall

            End If

            'Update paddle position.
            LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        End If

    End Sub

    Private Sub CheckLeftPaddleHit()

        'Did the ball hit the left paddle?
        If LeftPaddle.IntersectsWith(Ball) Then
            'Yes, the ball hit the left paddle.

            LeftPaddleHit = True

            If NumberOfPlayers = 2 Then

                VibrateRight(AControllerID, 30000)

            End If

            'Push the ball to the paddles right edge.
            BallPos.X = LeftPaddlePos.X + LeftPaddle.Width + 1

            'Update the ball position.
            Ball.X = Math.Round(BallPos.X)

            'BallPosX = LeftPaddle.X + LeftPaddle.Width + 1

            ApplyLeftPaddleEnglishToBall()

            PlayBounceSound()

        Else

            LeftPaddleHit = False

        End If

    End Sub

    Private Sub CheckRightPaddleHit()

        If RightPaddle.IntersectsWith(Ball) Then

            RightPaddleHit = True

            If NumberOfPlayers = 1 Then

                VibrateRight(AControllerID, 30000)

            Else

                VibrateRight(BControllerID, 30000)

            End If

            BallPos.X = RightPaddlePos.X - (Ball.Width + 5)

            'Update the ball position.
            Ball.X = Math.Round(BallPos.X)

            'BallPosX = RightPaddle.X - (Ball.Width + 5)

            ApplyRightPaddleEnglishToBall()

            PlayBounceSound()

        Else

            RightPaddleHit = False

            MouseWheelUp = False

            MouseWheelDown = False

        End If

    End Sub

    Private Sub ApplyLeftPaddleEnglishToBall()

        If NumberOfPlayers = 2 Then
            'The human player must manualy apply english to the ball
            'by pressing the controller ↑ ↓ buttons or the W S keys.

            If AControllerUp = True Then

                BallDirection = DirectionEnum.UpRight

            ElseIf AControllerDown = True Then

                BallDirection = DirectionEnum.DownRight

            ElseIf AControllerTsUp = True Then

                BallDirection = DirectionEnum.UpRight

            ElseIf AControllerTsDown = True Then

                BallDirection = DirectionEnum.DownRight

                'Is the left player holding W key down? 
            ElseIf WKeyDown = True Then 'W key moves left paddle up.
                'Yes, the left player is holding W key down.

                'Set the ball direction to up right.
                BallDirection = DirectionEnum.UpRight

                'Is the left player holding S key down?
            ElseIf SKeyDown = True Then 'S key moves left paddle down.
                'Yes, the left player is holding S key down.

                'Set the ball direction to down right.
                BallDirection = DirectionEnum.DownRight

            Else
                'The left player is not holding either W or S key down.
                'The left player is not holding the controller ↑ ↓ buttons down.

                'Set the ball direction to right.
                BallDirection = DirectionEnum.Right

            End If

        Else
            'For the computer player random english.
            'This makes the game more interesting.

            Select Case RandomNumber()
                Case 1
                    BallDirection = DirectionEnum.UpRight
                Case 2
                    BallDirection = DirectionEnum.Right
                Case 3
                    BallDirection = DirectionEnum.DownRight
            End Select

        End If

    End Sub

    Private Sub ApplyRightPaddleEnglishToBall()

        If NumberOfPlayers = 2 Then

            If BControllerUp = True Then

                BallDirection = DirectionEnum.UpLeft

            ElseIf BControllerDown = True Then

                BallDirection = DirectionEnum.DownLeft

            ElseIf BControllerTsUp = True Then

                BallDirection = DirectionEnum.UpLeft

            ElseIf BControllerTsDown = True Then

                BallDirection = DirectionEnum.DownLeft

            ElseIf MouseWheelUp = True Then

                BallDirection = DirectionEnum.UpLeft

                MouseWheelUp = False

            ElseIf MouseWheelDown = True Then

                BallDirection = DirectionEnum.DownLeft

                MouseWheelDown = False

            ElseIf UpArrowKeyDown = True Then

                BallDirection = DirectionEnum.UpLeft

            ElseIf DownArrowKeyDown = True Then

                BallDirection = DirectionEnum.DownLeft

            Else

                BallDirection = DirectionEnum.Left

            End If

        Else

            If AControllerUp = True Then

                BallDirection = DirectionEnum.UpLeft

            ElseIf AControllerDown = True Then

                BallDirection = DirectionEnum.DownLeft

            ElseIf AControllerTsUp = True Then

                BallDirection = DirectionEnum.UpLeft

            ElseIf AControllerTsDown = True Then

                BallDirection = DirectionEnum.DownLeft

            ElseIf MouseWheelUp = True Then

                BallDirection = DirectionEnum.UpLeft

                MouseWheelUp = False

            ElseIf MouseWheelDown = True Then

                BallDirection = DirectionEnum.DownLeft

                MouseWheelDown = False

            ElseIf UpArrowKeyDown = True Then

                BallDirection = DirectionEnum.UpLeft

            ElseIf DownArrowKeyDown = True Then

                BallDirection = DirectionEnum.DownLeft

            Else

                BallDirection = DirectionEnum.Left

            End If

        End If

    End Sub

    Private Sub UpdatePause()

        UpdateControllerPosition()

        If AControllerA = True Or BControllerA = True Then

            LastFrame = Now

            GameState = GameStateEnum.Playing

        End If

        If PKeyDown = True Or AKeyDown = True Then

            LastFrame = Now

            PKeyDown = False

            GameState = GameStateEnum.Playing

        End If

    End Sub

    Private Sub UpdateInstructions()

        UpdateControllerPosition()

        If NumberOfPlayers = 1 Then

            If AControllerB = True Or BControllerB = True Then

                SetupGame()

                PlayBounceSound()

            End If

            If AControllerX = True Or BControllerX = True Then

                SetupGame()

                PlayBounceSound()

            End If

            If SpaceBarDown = True Or BKeyDown = True Or XKeyDown = True Then

                SetupGame()

                PlayBounceSound()

            End If

        Else

            If AControllerA = True Or BControllerA = True Then

                SetupGame()

                PlayBounceSound()

            End If

            If SpaceBarDown = True Or AKeyDown = True Then

                SetupGame()

                PlayBounceSound()

            End If

        End If

    End Sub

    Private Sub SetupGame()

        LastFrame = Now

        GameState = GameStateEnum.Serve

    End Sub

    Private Sub UpdateStartScreen()

        UpdateControllerPosition()

        InstructStartLocation = New Point(ClientSize.Width \ 2, (ClientSize.Height \ 2) - 15)

        If AControllerA = True Or BControllerA = True Then

            NumberOfPlayers = 1

            GameState = GameStateEnum.Instructions

            PlayBounceSound()

        End If

        If AControllerB = True Or BControllerB = True Then

            NumberOfPlayers = 2

            GameState = GameStateEnum.Instructions

            PlayBounceSound()

        End If

        If AControllerX = True Or BControllerX = True Then

            NumberOfPlayers = 2

            GameState = GameStateEnum.Instructions

            PlayBounceSound()

        End If

        If OneKeyDown = True Or AKeyDown = True Then

            NumberOfPlayers = 1

            GameState = GameStateEnum.Instructions

            PlayBounceSound()

        End If

        If TwoKeyDown = True Or BKeyDown = True Or XKeyDown = True Then

            NumberOfPlayers = 2

            GameState = GameStateEnum.Instructions

            PlayBounceSound()

        End If

    End Sub

    Private Sub UpdateEndScreen()

        UpdateFlashingText()

        EndScreenCounter += 1

        If EndScreenCounter >= 300 Then

            ResetGame()

        End If

    End Sub

    Private Sub ResetGame()

        EndScreenCounter = 0

        LeftPaddleScore = 0

        RightPaddleScore = 0

        LeftPaddlePos.Y = ClientSize.Height \ 2 - LeftPaddle.Height \ 2 'Center verticaly

        'Update paddle position.
        LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        RightPaddlePos.X = ClientSize.Width - RightPaddle.Width - 20.0F 'Aline right 20 pix padding
        RightPaddlePos.Y = ClientSize.Height / 2.0F - RightPaddle.Height / 2.0F 'Center verticaly

        'Update paddle position.
        RightPaddle.X = Math.Round(RightPaddlePos.X)
        RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        PlaceBallCenterCourt()

        GameState = GameStateEnum.StartScreen

    End Sub

    Private Sub UpdateServe()

        PlaceBallCenterCourt()

        If Serving = ServeStateEnum.RightPaddle Then

            ServeRightPaddle()

        Else

            ServeLeftPaddle()

        End If

        GameState = GameStateEnum.Playing

    End Sub

    Private Sub PlaceBallCenterCourt()

        'Ball.Location = New Point((ClientSize.Width \ 2) - (Ball.Width \ 2), (ClientSize.Height \ 2) - (Ball.Height \ 2))
        BallPos.X = (ClientSize.Width \ 2) - (Ball.Width \ 2)
        BallPos.Y = (ClientSize.Height \ 2) - (Ball.Height \ 2)

        'Update the ball position.
        Ball.X = Math.Round(BallPos.X)
        Ball.Y = Math.Round(BallPos.Y)

    End Sub

    Private Sub ServeLeftPaddle()

        Select Case RandomNumber()

            Case 1

                BallDirection = DirectionEnum.UpRight

            Case 2

                BallDirection = DirectionEnum.Right

            Case 3

                BallDirection = DirectionEnum.DownRight

        End Select

    End Sub

    Private Sub ServeRightPaddle()

        Select Case RandomNumber()

            Case 1

                BallDirection = DirectionEnum.UpLeft

            Case 2

                BallDirection = DirectionEnum.Left

            Case 3

                BallDirection = DirectionEnum.DownLeft

        End Select

    End Sub

    Private Sub DrawPauseScreen()

        DrawCenterCourtLine()

        DrawLeftPaddle()

        DrawRightPaddle()

        DrawBall()

        DrawLeftPaddleScore()

        DrawRightPaddleScore()

        If NumberOfPlayers = 1 Then

            DrawComputerPlayerIdentifier()

        End If

        'Dim the frame.
        Buffer.Graphics.FillRectangle(DimmerBrush, ClientRectangle)

        DrawPausedText()

    End Sub

    Private Sub DrawEndScreen()

        DrawCenterCourtLine()

        DrawLeftPaddle()

        DrawRightPaddle()

        DrawBall()

        If NumberOfPlayers = 1 Then

            DrawComputerPlayerIdentifier()

        End If

        DrawEndScores()

    End Sub

    Private Sub DrawServe()

        DrawCenterCourtLine()

        DrawLeftPaddle()

        DrawRightPaddle()

        DrawBall()

        DrawLeftPaddleScore()

        DrawRightPaddleScore()

        If NumberOfPlayers = 1 Then

            DrawComputerPlayerIdentifier()

        End If

    End Sub

    Private Sub DrawStartScreen()

        DrawTitle()

        DrawStartScreenInstructions()

    End Sub

    Private Sub DrawInstructions()

        If NumberOfPlayers = 1 Then

            DrawTitle()

            'Draw one player instructions.
            Buffer.Graphics.DrawString(InstructOneText,
            InstructionsFont, Brushes.Orange, InstructOneLocation, AlineCenter)

        Else

            DrawTitle()

            'Draw two player instructions.
            Buffer.Graphics.DrawString(InstructTwoText,
            InstructionsFont, Brushes.Orange, InstructTwoLocation, AlineCenter)

        End If

    End Sub

    Private Sub DrawEndScores()

        'Did the left paddle win?
        If Winner = WinStateEnum.LeftPaddle Then
            'Yes, the left paddle won.

            'Flash the winning score.
            If DrawFlashingText = True Then

                DrawLeftPaddleScore()

            End If

        Else
            'No, the left paddle didn't win.

            DrawLeftPaddleScore()

        End If

        'Did the right paddle win?
        If Winner = WinStateEnum.RightPaddle Then
            'Yes, the right paddle won.


            'Flash the winning score.
            If DrawFlashingText = True Then

                DrawRightPaddleScore()

            End If

        Else
            'No, the right paddle didn't win.

            DrawRightPaddleScore()

        End If

    End Sub

    Private Sub UpdateControllerPosition()

        For ControllerNumber = 0 To 3 'Up to 4 controllers

            Try

                ' Check if the function call was successful
                If XInputGetState(ControllerNumber, ControllerPosition) = 0 Then
                    ' The function call was successful, so you can access the controller state now

                    UpdateButtonPosition()

                    UpdateLeftThumbstickPosition()

                    AssignController()

                Else
                    ' The function call failed, so you cannot access the controller state

                    'Text = "Failed to get controller state. Error code: " & XInputGetState(ControllerNumber, ControllerPosition).ToString

                    UnassignController()

                End If

            Catch ex As Exception

                MsgBox(ex.ToString)

                Exit Sub

            End Try

        Next

    End Sub

    Private Sub UpdateButtonPosition()
        'The range of buttons is 0 to 65,535. Unsigned 16-bit (2-byte) integer.

        'What buttons are down?
        Select Case ControllerPosition.Gamepad.wButtons
            Case 0 'All the buttons are up.
                If AControllerID = ControllerNumber Then
                    AControllerStart = False
                    AControllerA = False
                    AControllerB = False
                    AControllerX = False
                    AControllerUp = False
                    AControllerDown = False
                End If
                If BControllerID = ControllerNumber Then
                    BControllerStart = False
                    BControllerA = False
                    BControllerB = False
                    BControllerX = False
                    BControllerUp = False
                    BControllerDown = False
                End If
            Case 1 'Up
                If AControllerID = ControllerNumber Then
                    AControllerDown = False
                    AControllerUp = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerDown = False
                    BControllerUp = True
                End If
            Case 2 'Down
                If AControllerID = ControllerNumber Then
                    AControllerUp = False
                    AControllerDown = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerUp = False
                    BControllerDown = True
                End If
            Case 4 'Left
            Case 5 'Up+Left
                If AControllerID = ControllerNumber Then
                    AControllerDown = False
                    AControllerUp = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerDown = False
                    BControllerUp = True
                End If
            Case 6 'Down+Left
                If AControllerID = ControllerNumber Then
                    AControllerUp = False
                    AControllerDown = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerUp = False
                    BControllerDown = True
                End If
            Case 8 'Right
            Case 9 'Up+Right
                If AControllerID = ControllerNumber Then
                    AControllerDown = False
                    AControllerUp = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerDown = False
                    BControllerUp = True
                End If
            Case 10 'Down+Right
                If AControllerID = ControllerNumber Then
                    AControllerUp = False
                    AControllerDown = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerUp = False
                    BControllerDown = True
                End If
            Case 16 'Start
                If AControllerID = ControllerNumber Then
                    AControllerStart = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerStart = True
                End If
            Case 32 'Back
            Case 64 'Left Stick
            Case 128 'Right Stick
            Case 256 'Left bumper
            Case 512 'Right bumper
            Case 4096 'A
                If AControllerID = ControllerNumber Then
                    AControllerA = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerA = True
                End If
            Case 8192 'B
                If AControllerID = ControllerNumber Then
                    AControllerB = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerB = True
                End If
            Case 16384 'X
                If AControllerID = ControllerNumber Then
                    AControllerX = True
                End If
                If BControllerID = ControllerNumber Then
                    BControllerX = True
                End If
            Case 32768 'Y
        End Select

    End Sub

    Private Sub UpdateLeftThumbstickPosition()
        'The range on the X-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.
        'The range on the Y-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.

        'What position is the left thumbstick in on the X-axis?
        If ControllerPosition.Gamepad.sThumbLX <= NeutralStart Then
            'The left thumbstick is in the left position.
        ElseIf ControllerPosition.Gamepad.sThumbLX >= NeutralEnd Then
            'The left thumbstick is in the right position.
        Else
            'The left thumbstick is in the neutral position.
        End If

        'What position is the left thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbLY <= NeutralStart Then
            'The Left thumbstick is in the down position.

            If AControllerID = ControllerNumber Then
                AControllerTsUp = False
                AControllerTsDown = True
            End If
            If BControllerID = ControllerNumber Then
                BControllerTsUp = False
                BControllerTsDown = True
            End If

        ElseIf ControllerPosition.Gamepad.sThumbLY >= NeutralEnd Then
            'The left thumbstick is in the up position.

            If AControllerID = ControllerNumber Then
                AControllerTsDown = False
                AControllerTsUp = True
            End If
            If BControllerID = ControllerNumber Then
                BControllerTsDown = False
                BControllerTsUp = True
            End If

        Else
            'The left thumbstick is in the neutral position.

            If AControllerID = ControllerNumber Then
                AControllerTsUp = False
                AControllerTsDown = False
            End If
            If BControllerID = ControllerNumber Then
                BControllerTsUp = False
                BControllerTsDown = False
            End If

        End If

    End Sub

    Private Sub AssignController()
        'Assign controller a letter.

        'Has a controller been assigned to A?
        If AControllerID < 0 Then
            'No controller been assigned to A.

            'Is the controller assigned to B?
            If BControllerID <> ControllerNumber Then
                'No, the controller is not assigned to B.

                'Assign controller to A.
                AControllerID = ControllerNumber

            End If

        End If

        'Has a controller been assigned to B?
        If BControllerID < 0 Then
            'No, a controller has not been assigned to B.

            'Is the controller assigned to A?
            If AControllerID <> ControllerNumber Then
                'No, the controller is not assigned to A.

                'Assign controller to B.
                BControllerID = ControllerNumber

            End If

        End If

    End Sub

    Private Sub UnassignController()

        'Is this the controller to unassign?
        If AControllerID = ControllerNumber Then
            'Yes, this is the one.

            'Unassign
            AControllerID = -1

        End If

        'Is this the controller to unassign?
        If BControllerID = ControllerNumber Then
            'Yes, this is the one.

            'Unassign
            BControllerID = -1

        End If

    End Sub

    Private Sub DrawComputerPlayerIdentifier()

        Buffer.Graphics.DrawString("CPU", InstructionsFont, Brushes.White, ClientSize.Width \ 2 \ 2, 20, AlineCenterMiddle)

    End Sub

    Private Sub DrawRightPaddleScore()

        Buffer.Graphics.DrawString(RightPaddleScore, RightScore0Font, GreenGlow0Brush, RPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(RightPaddleScore, RightScore1Font, GreenGlow1Brush, RPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(RightPaddleScore, RightScore2Font, GreenGlow2Brush, RPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(RightPaddleScore, RightScore3Font, GreenGlow3Brush, RPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(RightPaddleScore, RightScore4Font, GreenGlow4Brush, RPadScoreLocation, AlineCenterMiddle)

    End Sub

    Private Sub DrawLeftPaddleScore()

        Buffer.Graphics.DrawString(LeftPaddleScore, LeftScore0Font, LeftScore0Brush, LPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(LeftPaddleScore, LeftScore1Font, LeftScore1Brush, LPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(LeftPaddleScore, LeftScore2Font, LeftScore2Brush, LPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(LeftPaddleScore, LeftScore3Font, LeftScore3Brush, LPadScoreLocation, AlineCenterMiddle)
        Buffer.Graphics.DrawString(LeftPaddleScore, LeftScore4Font, LeftScore4Brush, LPadScoreLocation, AlineCenterMiddle)

    End Sub

    Private Sub MoveBallRight()

        BallPos.X += BallVelocity * DeltaTime.TotalSeconds

        'Update the ball position.
        Ball.X = Math.Round(BallPos.X)

    End Sub

    Private Sub MoveBallLeft()

        BallPos.X -= BallVelocity * DeltaTime.TotalSeconds

        'Update the ball position.
        Ball.X = Math.Round(BallPos.X)

    End Sub

    Private Sub MoveBallDown()

        BallPos.Y += BallVelocity * DeltaTime.TotalSeconds

        'Update the ball position.
        Ball.Y = Math.Round(BallPos.Y)

    End Sub

    Private Sub MoveBallUp()

        BallPos.Y -= BallVelocity * DeltaTime.TotalSeconds

        'Update the ball position.
        Ball.Y = Math.Round(BallPos.Y)

    End Sub

    Private Sub MoveBallDownRight()

        MoveBallRight()

        MoveBallDown()

        'Did the ball hit the bottom wall?
        If BallPos.Y + Ball.Height > BottomWall Then
            'Yes, the ball hit the bottom wall.

            BallDirection = DirectionEnum.UpRight

            PlayBounceSound()

        End If

    End Sub

    Private Sub MoveBallUpRight()

        MoveBallRight()

        MoveBallUp()

        'Did the ball hit the top wall?
        If BallPos.Y < TopWall Then
            'Yes, the ball hit the top wall.

            BallDirection = DirectionEnum.DownRight

            PlayBounceSound()

        End If

    End Sub

    Private Sub MoveBallDownLeft()

        MoveBallLeft()

        MoveBallDown()

        'Did the ball hit the bottom wall?
        If BallPos.Y + Ball.Height > BottomWall Then
            'Yes, the ball hit the bottom wall.

            BallDirection = DirectionEnum.UpLeft

            PlayBounceSound()

        End If

    End Sub

    Private Sub MoveBallUpLeft()

        MoveBallLeft()

        MoveBallUp()

        'Did the ball hit the top wall?
        If BallPos.Y < TopWall Then
            'Yes, the ball hit the top wall.

            BallDirection = DirectionEnum.DownLeft

            PlayBounceSound()

        End If

    End Sub

    Private Sub InitializePaddles()

        LeftPaddle.Width = 25
        LeftPaddle.Height = 100

        LeftPaddlePos.X = 20
        LeftPaddlePos.Y = ClientSize.Height \ 2 - LeftPaddle.Height \ 2 'Center vertically

        'Update paddle position.
        LeftPaddle.X = Math.Round(LeftPaddlePos.X)
        LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        RightPaddle.Width = 25
        RightPaddle.Height = 100
        RightPaddlePos.X = ClientSize.Width - RightPaddle.Width - 20.0F 'Aline right 20 pix padding
        RightPaddlePos.Y = ClientSize.Height / 2.0F - RightPaddle.Height / 2.0F 'Center vertically

        'Update paddle position.
        RightPaddle.X = Math.Round(RightPaddlePos.X)
        RightPaddle.Y = Math.Round(RightPaddlePos.Y)

    End Sub

    Private Sub InitializeBall()

        Ball.Width = 26
        Ball.Height = 26

        PlaceBallCenterCourt()

    End Sub

    Private Sub LayoutGame()

        'Center the left paddle vertically in the forms client area.
        LeftPaddlePos.Y = ClientSize.Height \ 2 - LeftPaddle.Height \ 2

        'Update paddle position.
        LeftPaddle.Y = Math.Round(LeftPaddlePos.Y)

        'Center the right paddle vertically in the forms client area.
        RightPaddlePos.Y = ClientSize.Height / 2.0F - RightPaddle.Height / 2.0F

        'Aline the right paddle along the right side of the form client area allow 20
        'pixels padding.
        RightPaddlePos.X = ClientSize.Width - RightPaddle.Width - 20.0F

        'Update paddle position.
        RightPaddle.X = Math.Round(RightPaddlePos.X)
        RightPaddle.Y = Math.Round(RightPaddlePos.Y)

        CenterCourtLine()

        BottomWall = ClientSize.Height

        TitleLocation = New Point(ClientSize.Width \ 2, ClientSize.Height \ 2 - 50)

        LPadScoreLocation = New Point(ClientSize.Width \ 2 \ 2, 100)

        RPadScoreLocation = New Point(ClientSize.Width - (ClientSize.Width \ 4), 100)

        LayoutInstructions()

        ClientCenter = New Point(ClientSize.Width \ 2, ClientSize.Height \ 2)

    End Sub

    Private Sub UpdateFlashingText()
        'This algorithm controls the rate of flash for text.

        'Advance the frame counter.
        FlashCount += 1

        'Draw text for 20 frames.
        If FlashCount <= 20 Then

            DrawFlashingText = True

        Else

            DrawFlashingText = False

        End If

        'Dont draw text for the next 20 frames.
        If FlashCount >= 40 Then

            'Repete
            FlashCount = 0

        End If

    End Sub

    Private Sub DrawStartScreenInstructions()

        Buffer.Graphics.DrawString(InstructStartText, InstructionsFont, Brushes.Orange, InstructStartLocation, AlineCenter)

    End Sub

    Private Sub DrawPausedText()

        'Draw paused text.
        Buffer.Graphics.DrawString("Paused", TitleFont, Brushes.White, ClientCenter, AlineCenterMiddle)

        Buffer.Graphics.DrawString("Resume:  A", InstructionsFont, Brushes.Orange, InstructPauseLocation, AlineCenterMiddle)

    End Sub

    Private Sub DrawBall()

        DrawGlowingOrchid(Ball)

    End Sub

    Private Sub DrawRightPaddle()

        If RightPaddleHit = True Then

            RightPaddleHitTimer = 10

        End If

        If RightPaddleHitTimer = 0 Then

            DrawGlowingSkyBlue(RightPaddle)

        Else

            DrawGlowingOrchid(RightPaddle)

            RightPaddleHitTimer -= 1

        End If

    End Sub

    Private Sub DrawLeftPaddle()

        If LeftPaddleHit = True Then

            LeftPaddleHitTimer = 10

        End If

        If LeftPaddleHitTimer = 0 Then

            DrawGlowingSkyBlue(LeftPaddle)

        Else

            DrawGlowingOrchid(LeftPaddle)

            LeftPaddleHitTimer -= 1

        End If

    End Sub

    Private Sub DrawCenterCourtLine()

        Buffer.Graphics.DrawLine(CenterlinePen, CenterlineTop, CenterlineBottom)

    End Sub

    Private Sub DrawTitle()

        Dim Loc As Point

        Loc = TitleLocation

        Loc.Offset(-272, 0)

        DrawTitleGlow("G", Loc)

        Loc = TitleLocation

        Loc.Offset(-205, 0)

        DrawTitleGlow("L", Loc)

        Loc = TitleLocation

        Loc.Offset(-140, 0)

        DrawTitleGlow("O", Loc)

        Loc = TitleLocation

        Loc.Offset(-54, 0)

        DrawTitleGlow("W", Loc)

        Loc = TitleLocation

        Loc.Offset(52, 0)

        DrawTitleGlow("P", Loc)

        Loc = TitleLocation

        Loc.Offset(124, 0)

        DrawTitleGlow("O", Loc)

        Loc = TitleLocation

        Loc.Offset(198, 0)

        DrawTitleGlow("N", Loc)

        Loc = TitleLocation

        Loc.Offset(272, 0)

        DrawTitleGlow("G", Loc)

        'Buffer.Graphics.DrawString(TitleText, TitleFont, Brushes.Orange, TitleLocation, AlineCenterMiddle)

    End Sub

    Private Sub DrawTitleGlow(Text As String, Location As Point)

        Buffer.Graphics.DrawString(Text, TitleGlow0Font, GreenGlow0Brush, Location, AlineCenterMiddle)
        Buffer.Graphics.DrawString(Text, TitleGlow1Font, GreenGlow1Brush, Location, AlineCenterMiddle)
        Buffer.Graphics.DrawString(Text, TitleGlow2Font, GreenGlow2Brush, Location, AlineCenterMiddle)
        Buffer.Graphics.DrawString(Text, TitleGlow3Font, GreenGlow3Brush, Location, AlineCenterMiddle)
        Buffer.Graphics.DrawString(Text, TitleGlow4Font, GreenGlow4Brush, Location, AlineCenterMiddle)

    End Sub

    Private Sub CenterCourtLine()

        'Centers the court line in the client area of our form.
        CenterlineTop = New Point(ClientSize.Width \ 2, 0)

        CenterlineBottom = New Point(ClientSize.Width \ 2, ClientSize.Height)

    End Sub

    Private Sub LayoutInstructions()

        Dim Location As New Point(ClientSize.Width \ 2, (ClientSize.Height \ 2) - 15)

        InstructOneLocation = Location

        InstructTwoLocation = Location

        InstructPauseLocation = New Point(ClientSize.Width \ 2, (ClientSize.Height \ 2) + 75)

    End Sub

    Private Sub InitializeForm()

        WindowState = FormWindowState.Maximized

        Text = "Glow Pong - Code with Joe"

        SetStyle(ControlStyles.AllPaintingInWmPaint, True) ' True is better
        SetStyle(ControlStyles.OptimizedDoubleBuffer, True) ' True is better

    End Sub

    Private Sub InitializeStringAlinement()

        AlineCenter.Alignment = StringAlignment.Center
        AlineCenterMiddle.Alignment = StringAlignment.Center
        AlineCenterMiddle.LineAlignment = StringAlignment.Center

    End Sub

    Private Shared Sub PlayBounceSound()

        My.Computer.Audio.Play(My.Resources.bounce, AudioPlayMode.Background)

        'Used Audacity to generate tone.
        'Frequency:600Hz  Amplitude:0.1  Duration:0.183s
        'saved as bounce.wav.

    End Sub

    Private Shared Sub PlayScoreSound()

        My.Computer.Audio.Play(My.Resources.score, AudioPlayMode.Background)

    End Sub

    Private Shared Sub PlayWinningSound()

        My.Computer.Audio.Play(My.Resources.winning, AudioPlayMode.Background)

    End Sub

    Private Shared Function RandomNumber() As Integer

        'Initialize random-number generator.
        Randomize()

        'Generate random number between 1 and 3.
        Return CInt(Int((3 * Rnd()) + 1))

    End Function

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        DoKeyDown(e)

    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp

        DoKeyUp(e)

    End Sub

    Private Sub Form1_MouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseWheel

        'Is the player rolling the mouse wheel up?
        If e.Delta > 0 Then
            'Yes, the player is rolling the mouse wheel up.

            MouseWheelDown = False

            MouseWheelUp = True

        Else
            'No, the player is not rolling the mouse wheel up.

            MouseWheelUp = False

            MouseWheelDown = True

        End If

    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove

        'Are the players playing?
        If GameState = GameStateEnum.Playing Then
            'Yes, the players are playing.

            'Move mouse pointer off the play area.
            Cursor.Position = PointToScreen(New Point(ClientRectangle.Width - 3, ClientRectangle.Height \ 2))

        End If

    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        If WindowState = FormWindowState.Minimized Then

            If GameState = GameStateEnum.Playing Then

                GameState = GameStateEnum.Pause

            End If

        End If

        LayoutGame()

    End Sub

    Private Sub DoKeyDown(e As KeyEventArgs)
        'When the player pushes a key down do the following...

        Select Case e.KeyCode

                'Did player push space bar?
            Case Keys.Space
                'Yes, player did push space bar.

                SpaceBarDown = True

                'Did player push W key?
            Case Keys.W
                'Yes, player did push W key.

                WKeyDown = True

                'Did player push S key?
            Case Keys.S
                'Yes, player did push S key.

                SKeyDown = True

                'Did player push up arrow key?
            Case Keys.Up
                'Yes, player did push up arrow key.

                UpArrowKeyDown = True

                'Did player push down arrow key?
            Case Keys.Down
                'Yes, player did push down arrow key.

                DownArrowKeyDown = True

                'Did player push down the 1 key?
            Case Keys.D1
                'Yes, player did push down the 1 key.

                OneKeyDown = True

                'Did player push down the 1 key on number pad?
            Case Keys.NumPad1
                'Yes, player did push down the 1 key on number pad.

                OneKeyDown = True

                'Did player push down the 2 key?
            Case Keys.D2
                'Yes, player did push down the 2 key.

                TwoKeyDown = True

                'Did player push down the 2 key on number pad?
            Case Keys.NumPad2
                'Yes, player did push down the 2 key on number pad.

                TwoKeyDown = True

                'Did player push down the p key?
            Case Keys.P
                'Yes, player did push down the p key.

                PKeyDown = True

                'Did player push down the a key?
            Case Keys.A
                'Yes, player did push down the a key.

                AKeyDown = True

                'Did player push down the b key?
            Case Keys.B
                'Yes, player did push down the b key.

                BKeyDown = True

                'Did player push down the x key?
            Case Keys.X
                'Yes, player did push down the x key.

                XKeyDown = True

        End Select

    End Sub

    Private Sub DoKeyUp(e As KeyEventArgs)
        'When the player lets a key up do the following...

        Select Case e.KeyCode

                'Did the player let the space bar up?
            Case Keys.Space
                'Yes, the player let the space bar up?

                SpaceBarDown = False

            Case Keys.W

                WKeyDown = False

            Case Keys.S

                SKeyDown = False

            Case Keys.Up

                UpArrowKeyDown = False

            Case Keys.Down

                DownArrowKeyDown = False

            Case Keys.D1 'The 1 key.

                OneKeyDown = False

            Case Keys.NumPad1 'The 1 key on number pad.

                OneKeyDown = False

            Case Keys.D2  'The 2 key.

                TwoKeyDown = False

            Case Keys.NumPad2 'The 2 key on number pad.

                TwoKeyDown = False

                'Did player let the p key up?
            Case Keys.P
                'Yes, player did let the p key up.
                PKeyDown = False

                'Did player let the a key up?
            Case Keys.A
                'Yes, player did let the a key up.

                AKeyDown = False

                'Did player let the b key up?
            Case Keys.B
                'Yes, player did push down the b key.

                BKeyDown = False

                'Did player let the x key up?
            Case Keys.X
                'Yes, player let the x key up.

                XKeyDown = False

        End Select

    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

    Private Sub VibrateRight(ByVal ControllerNumber As Integer, ByVal Speed As UShort)
        'The range of speed is 0 through 65,535. Unsigned 16-bit (2-byte) integer.
        'The right motor is the high-frequency rumble motor.

        'Turn left motor off (set zero speed).
        Vibration.wLeftMotorSpeed = 0

        'Set right motor speed.
        Vibration.wRightMotorSpeed = Speed

        Vibrate(ControllerNumber)

    End Sub

    Private Sub Vibrate(ByVal ControllerNumber As Integer)

        Try

            'Turn motor on.
            If XInputSetState(ControllerNumber, Vibration) = 0 Then
                'Success
                'Text = XInputSetState(ControllerNumber, vibration).ToString
            Else
                'Fail
                'Text = XInputSetState(ControllerNumber, vibration).ToString
            End If

        Catch ex As Exception

            MsgBox(ex.ToString)

            Exit Sub

        End Try

    End Sub

    Private Sub DrawGlowingOrchid(Rect As Rectangle)

        Buffer.Graphics.DrawRectangle(Orchid0Pen, Rect)
        Buffer.Graphics.DrawRectangle(Orchid1Pen, Rect)
        Buffer.Graphics.DrawRectangle(Orchid2Pen, Rect)
        Buffer.Graphics.DrawRectangle(Orchid3Pen, Rect)
        Buffer.Graphics.DrawRectangle(Orchid4Pen, Rect)

    End Sub

    Private Sub DrawGlowingSkyBlue(Rect As Rectangle)

        Buffer.Graphics.DrawRectangle(SkyBlue0Pen, Rect)
        Buffer.Graphics.DrawRectangle(SkyBlue1Pen, Rect)
        Buffer.Graphics.DrawRectangle(SkyBlue2Pen, Rect)
        Buffer.Graphics.DrawRectangle(SkyBlue3Pen, Rect)
        Buffer.Graphics.DrawRectangle(SkyBlue4Pen, Rect)

    End Sub

End Class


'Learn more:
'
'Consuming Unmanaged DLL Functions
'https://learn.microsoft.com/en-us/dotnet/framework/interop/consuming-unmanaged-dll-functions
'
'XInputGetState Function
'https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetstate
'
'XINPUT_STATE Structure
'https://learn.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_state
'
'XINPUT_GAMEPAD Structure
'https://learn.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_gamepad
'
'XInputSetState Function
'https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputsetstate
'
'XINPUT_VIBRATION Structure
'https://learn.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_vibration
'
'Getting Started with XInput in Windows Applications
'https://learn.microsoft.com/en-us/windows/win32/xinput/getting-started-with-xinput
'
'XInput Game Controller APIs
'https://learn.microsoft.com/en-us/windows/win32/api/_xinput/
'
'XInput Versions
'https://learn.microsoft.com/en-us/windows/win32/xinput/xinput-versions
'
'Comparison of XInput and DirectInput Features
'https://learn.microsoft.com/en-us/windows/win32/xinput/xinput-and-directinput
'
'Short Data Type
'https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/data-types/short-data-type
'
'Byte Data Type
'https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/data-types/byte-data-type
'
'UShort Data Type
'https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/data-types/ushort-data-type
'
'UInteger Data Type
'https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/data-types/uinteger-data-type
'
'Boolean Data Type
'https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/data-types/boolean-data-type
'
'Integer Data Type
'https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/data-types/integer-data-type
'
'DllImportAttribute.EntryPoint Field
'https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute.entrypoint?view=net-7.0
'
'Passing Structures
'https://learn.microsoft.com/en-us/dotnet/framework/interop/passing-structures
'
'Strings used in Structures
'https://learn.microsoft.com/en-us/dotnet/framework/interop/default-marshalling-for-strings#strings-used-in-structures
'
'TextureBrush Class
'https://learn.microsoft.com/en-us/dotnet/api/system.drawing.texturebrush?view=windowsdesktop-7.0&source=recommendations
'
'