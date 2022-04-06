using Sandbox;
using System;
using System.Threading.Tasks;

namespace Willco2;
public static class Ext {
	public static void DebugLog<T>(this T any) => Log.Info(any.ToString());
	public static void DebugText<T>(this T any, int line = 0, float duration = 0) => DebugOverlay.ScreenText(line, any.ToString(), duration);

	public static Vector3 MoveTowards(this Vector3 start, Vector3 target, float maxDistance) {
		float distance = start.Distance(target);
		maxDistance = maxDistance.Clamp(0, distance);
		if (distance == 0) return target;

		Vector3 normal = (target - start) / distance;
		return start + normal * maxDistance;
	}

	public static Vector3 NearestPointOnLine(this Vector3 point, Vector3 lineOrigin, Vector3 unitDirection) {
		var v = point - lineOrigin;
		var d = Vector3.Dot(v, unitDirection);
		return lineOrigin + unitDirection * d;
	}
	public static Vector3 DistanceToLine(this Vector3 point, Vector3 lineOrigin, Vector3 unitDirection) => Vector3.DistanceBetween(point, point.NearestPointOnLine(lineOrigin, unitDirection));

	public static Vector3 RandomPointOnSurface(this Sphere sphere) {
		float theta = Rand.Float(MathF.PI * 2), phi = MathF.Acos(2 * Rand.Float() - 1);
		return new Vector3(MathF.Cos(theta) * MathF.Sin(phi), MathF.Sin(theta) * MathF.Sin(phi), MathF.Cos(phi)) * sphere.Radius + sphere.Center;
	}

	public static Transform RotateAround(in this Transform tx, Vector3 origin, Rotation rotation) => tx with { Position = rotation * (tx.Position - origin) + origin, Rotation = rotation * tx.Rotation };
	public static Vector3 RotateAround(this Vector3 pos, Vector3 pivot, Rotation rot) => rot * (pos - pivot) + pivot;

	public static float ToFloat(this bool boolean) => boolean ? 1f : 0f;
	public static int ToInt(this bool boolean) => boolean ? 1 : 0;
	public static int ToSignedInt(this bool boolean) => boolean ? 1 : -1;
	public static int Round(this float num) => (int) MathF.Round(num, MidpointRounding.AwayFromZero);

	public static Vector3 Sign(this Vector3 vec) => new(Math.Sign(vec.x), Math.Sign(vec.y), Math.Sign(vec.z));
	public static float SquaredDistance(this Vector3 start, Vector3 end) => (start - end).LengthSquared;
	public static float Sin(this float num, float frequency = 1) => MathF.Sin(num * frequency);
	public static float Cos(this float num, float frequency = 1) => MathF.Cos(num * frequency);
	public static float Oscillate(this float num, float lower = 0, float upper = 1, float frequency = 1) => MathF.Sin(num * frequency) * 0.5f * (upper - lower) + (upper + lower) * 0.5f;
	public static float Abs(this float num) => MathF.Abs(num);
	public static float Max(this float num, float other) => MathF.Max(num, other);
	public static float Min(this float num, float other) => MathF.Min(num, other);
	public static float FastInvSqrt(this float num) {
		float xhalf = 0.5f * num;
		int i = BitConverter.SingleToInt32Bits(num);
		i = 0x5f375a86 - (i >> 1);
		num = BitConverter.Int32BitsToSingle(i);
		num = num * (1.5f - xhalf * num * num);
		return num;
	}

	public static void StartAfter(this TaskSource source, float seconds, Action action) => _ = startAfter(seconds, action);
	private static async Task startAfter(float seconds, Action action) {
		await GameTask.DelaySeconds(seconds);
		action();
	}

	public static float GaussianRandom(this (float mean, float stddev) dist) {
		// The method requires sampling from a uniform random of (0,1]
		// but Random.NextDouble() returns a sample of [0,1).
		float x1 = 1 - Rand.Float();
		float x2 = 1 - Rand.Float();
		float y1 = MathF.Sqrt(-2 * MathF.Log(x1)) * MathF.Cos(2 * MathF.PI * x2);
		return y1 * dist.stddev + dist.mean;
	}

	// If a check were run every tick:
	// if (Rand.Float() > chanceToIterate) return;
	// Then this function approximates a probability-accurate amount of seconds before the check fails, and the code below it is allowed to execute
	public static float IterativeDurationRandom(this float chanceToIterate) {
		return MathF.Log(1 - Rand.Float().Clamp(0.0001f, 0.9999f), 1 - chanceToIterate) * Global.TickInterval;
	}

	public static Vector3 GetFaceNormalAt(this BBox box, Vector3 pos) =>
		new Vector3(pos.x > box.Maxs.x ? 1 : (pos.x < box.Mins.x ? -1 : 0), pos.y > box.Maxs.y ? 1 : (pos.y < box.Mins.y ? -1 : 0), pos.z > box.Maxs.z ? 1 : (pos.z < box.Mins.z ? -1 : 0)).Normal;

}
