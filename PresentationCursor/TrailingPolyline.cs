using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace PresentationCursor
{
    public class TrailingPolyline
    {
        private readonly Canvas _canvas;
        private readonly Queue<TrailSegment> _trailSegments = new();
        private Point? _lastPoint;
        
        // Configuration properties
        public int MaxTrailSegments { get; set; } = 50;
        public double FadeRate { get; set; } = 0.025;
        public double MinMovementDistance { get; set; } = 3.0;
        
        public TrailingPolyline(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }
        
        /// <summary>
        /// Updates the trail with a new cursor position
        /// </summary>
        /// <param name="currentPoint">The current cursor position</param>
        public void OnTick(Point currentPoint)
        {
            // Add new trail segment if we have a previous point and moved enough
            if (_lastPoint.HasValue && 
                (Math.Abs(currentPoint.X - _lastPoint.Value.X) > MinMovementDistance || 
                 Math.Abs(currentPoint.Y - _lastPoint.Value.Y) > MinMovementDistance))
            {
                var segment = new TrailSegment(_lastPoint.Value, currentPoint);
                _trailSegments.Enqueue(segment);
                _canvas.Children.Add(segment.Line);
                
                // Limit number of segments
                while (_trailSegments.Count > MaxTrailSegments)
                {
                    var oldSegment = _trailSegments.Dequeue();
                    _canvas.Children.Remove(oldSegment.Line);
                }
            }
            
            _lastPoint = currentPoint;
            
            // Update all existing segments
            UpdateSegments();
        }
        
        /// <summary>
        /// Called when cursor leaves the tracking area - resets the last point to avoid connecting segments across screens
        /// </summary>
        public void OnCursorLeft()
        {
            _lastPoint = null;
            UpdateSegments(); // Continue updating existing segments
        }
        
        /// <summary>
        /// Clears all trail segments
        /// </summary>
        public void Clear()
        {
            foreach (var segment in _trailSegments)
            {
                _canvas.Children.Remove(segment.Line);
            }
            _trailSegments.Clear();
            _lastPoint = null;
        }
        
        private void UpdateSegments()
        {
            var segmentsToRemove = new List<TrailSegment>();
            
            foreach (var segment in _trailSegments)
            {
                segment.Age += 1;
                
                // Calculate opacity based on age (newer segments are more opaque)
                double opacity = Math.Max(0, 1.0 - (segment.Age * FadeRate));
                segment.Line.Opacity = opacity;

                // Change color as it fades (yellow -> orange -> red)
                var ageRatio = segment.Age * FadeRate;
                if (ageRatio < 0.5)
                {
                    // Yellow to Orange
                    var color = Color.FromRgb(255, (byte)(255 * (1 - ageRatio * 2)), 0);
                    segment.Line.Stroke = new SolidColorBrush(color);
                }
                
                // Make the line thinner as it ages
                segment.Line.StrokeThickness = Math.Max(1, TrailSegment.DefaultThickness * (1 - ageRatio * 0.5));
                
                if (opacity <= 0)
                {
                    segmentsToRemove.Add(segment);
                }
            }
            
            // Remove fully faded segments
            foreach (var segment in segmentsToRemove)
            {
                _canvas.Children.Remove(segment.Line);
            }
            
            // Rebuild queue without faded segments
            if (segmentsToRemove.Count > 0)
            {
                var remainingSegments = _trailSegments.Where(s => !segmentsToRemove.Contains(s)).ToArray();
                _trailSegments.Clear();
                foreach (var segment in remainingSegments)
                {
                    _trailSegments.Enqueue(segment);
                }
            }
        }
    }
}