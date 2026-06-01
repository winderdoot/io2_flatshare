import { useQuery } from "@tanstack/react-query";
import { useState, useEffect } from "react";
import { getListingThumbnail } from "../images_service/ImagesService";

export const useListingThumbnail = (listingId: string) => {
  const token = localStorage.getItem("token") || "";

  const { data: blob, isLoading } = useQuery({
    queryKey: ["listing-thumbnail", listingId],
    queryFn: () => getListingThumbnail(listingId, token),
    staleTime: 1000 * 60 * 10,
  });

  const [imageUrl, setImageUrl] = useState<string | null>(null);

  useEffect(() => {
    if (!blob) {
      setImageUrl(null);
      return;
    }

    const objectUrl = URL.createObjectURL(blob);
    setImageUrl(objectUrl);

    console.log("Co powinno byc:", objectUrl);

    return () => {
      URL.revokeObjectURL(objectUrl);
    };
  }, [blob]);

  return { imageUrl, isLoading };
};