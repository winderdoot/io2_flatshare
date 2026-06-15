import { API_URL } from "../config.ts";

type PhotosResponse = {
  listingId?: string;
  ListingId?: string;
  photos?: string[];
  Photos?: string[];
};

function photoHeaders(token?: string): HeadersInit {
  const headers: Record<string, string> = { Accept: "application/json" };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  return headers;
}

function extractPhotoIds(data: PhotosResponse): string[] {
  const raw = data.photos ?? data.Photos;
  if (!Array.isArray(raw)) return [];
  return raw.map(String);
}

export const getListingThumbnail = async (
  listingId: string,
  token?: string
): Promise<Blob | null> => {
  try {
    const photosResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos`,
      { headers: photoHeaders(token) }
    );

    if (!photosResponse.ok) {
      return null;
    }

    const photosData = (await photosResponse.json()) as PhotosResponse;
    const firstPhotoId = extractPhotoIds(photosData)[0];
    if (!firstPhotoId) {
      return null;
    }

    const imageResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos/${firstPhotoId}`,
      { headers: photoHeaders(token) }
    );

    if (!imageResponse.ok) {
      return null;
    }

    return await imageResponse.blob();
  } catch {
    return null;
  }
};

export type ListingPhoto = {
  id: string;
  blob: Blob | null;
};

export const getListingPhotos = async (
  listingId: string,
  token?: string
): Promise<ListingPhoto[]> => {
  try {
    const photosResponse = await fetch(
      `${API_URL}/api/v1/listings/${listingId}/photos`,
      {
        cache: "no-store",
        headers: photoHeaders(token),
      }
    );

    if (!photosResponse.ok) return [];

    const photosData = (await photosResponse.json()) as PhotosResponse;
    const photoIds = extractPhotoIds(photosData);
    if (!photoIds.length) return [];

    const images = await Promise.all(
      photoIds.map(async (photoId) => {
        const imageResponse = await fetch(
          `${API_URL}/api/v1/listings/${listingId}/photos/${photoId}`,
          {
            cache: "no-store",
            headers: photoHeaders(token),
          }
        );

        if (!imageResponse.ok) return { id: photoId, blob: null };

        const blob = await imageResponse.blob();
        return { id: photoId, blob };
      })
    );

    return images.filter((image) => image.blob !== null);
  } catch {
    return [];
  }
};

export const deleteListingPhoto = async (
  listingId: string,
  photoId: string,
  token: string
): Promise<void> => {
  const response = await fetch(
    `${API_URL}/api/v1/listings/${listingId}/photos/${photoId}`,
    {
      method: "DELETE",
      headers: photoHeaders(token),
    }
  );

  if (!response.ok && response.status !== 204) {
    throw new Error("Error deleting photo");
  }
};

export const uploadListingPhoto = async (
  listingId: string,
  file: File,
  token: string
): Promise<string> => {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(
    `${API_URL}/api/v1/listings/${listingId}/photos`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: "application/json",
      },
      body: formData,
    }
  );

  if (!response.ok) {
    throw new Error("Error uploading photo");
  }

  const location = response.headers.get("Location");
  if (location) {
    return location.split("/").pop() ?? "";
  }

  try {
    const data = (await response.json()) as { id?: string; Id?: string };
    return String(data.id ?? data.Id ?? "");
  } catch {
    return "";
  }
};
